using Dtd.Application.Almacenes;
using Dtd.Application.Documentos.Contracts;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Mapping;
using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.GenerarDocumento;

internal sealed class GenerarDocumentoCommandHandler
    : IRequestHandler<GenerarDocumentoCommand, ErrorOr<Guid>>
{
    private readonly IExpedicionErpGateway _erpGateway;
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IDocumentReferenceGenerator _documentReferenceGenerator;

    private readonly IAccesoAlmacenService _accesoAlmacenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public GenerarDocumentoCommandHandler(
        IExpedicionErpGateway erpGateway,
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IDocumentReferenceGenerator documentReferenceGenerator,
        IAccesoAlmacenService accesoAlmacenService,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _erpGateway = erpGateway;
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _documentReferenceGenerator = documentReferenceGenerator;
        _accesoAlmacenService = accesoAlmacenService;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<Guid>> Handle(
        GenerarDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        var accesoAlmacen = await _accesoAlmacenService.ValidarAccesoAsync(
            request.Empresa,
            request.AlmacenId,
            cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        var rango = RangoFechas.Create(
            request.FechaDesde,
            request.FechaHasta);

        var configuracion = await ObtenerConfiguracionAsync(
            request,
            cancellationToken);

        if (configuracion.IsError)
        {
            return configuracion.Errors;
        }

        var (almacen, agencia) = configuracion.Value;

        var resultadoExpedicionesErp =
            await ObtenerExpedicionesErpAsync(
                request.Empresa,
                almacen,
                agencia,
                rango,
                cancellationToken);

        if (resultadoExpedicionesErp.IsError)
        {
            return resultadoExpedicionesErp.Errors;
        }

        var resultadoNuevas =
            await ObtenerExpedicionesNuevasAsync(
                request.Empresa,
                almacen,
                agencia,
                resultadoExpedicionesErp.Value,
                cancellationToken);

        if (resultadoNuevas.IsError)
        {
            return resultadoNuevas.Errors;
        }

        var nuevas = resultadoNuevas.Value;

        var expediciones = nuevas
            .Select(dto => dto.ToDomain(
                almacen.Id,
                agencia.Id))
            .ToList();

        var tipoAgrupacion = agencia.EnvioDirecto
            ? TipoAgrupacionEnvio.PorAlmacenDestino
            : TipoAgrupacionEnvio.UnicoPorAgencia;

        DestinoEnvio? destinoAgencia = null;

        IReadOnlyDictionary<string, DestinoEnvio> destinosAlmacen =
            new Dictionary<string, DestinoEnvio>(
                StringComparer.OrdinalIgnoreCase);

        switch (tipoAgrupacion)
        {
            case TipoAgrupacionEnvio.UnicoPorAgencia:
                {
                    var resultadoDestinoAgencia =
                        await ObtenerDestinoAgenciaAsync(
                            almacen,
                            agencia,
                            cancellationToken);

                    if (resultadoDestinoAgencia.IsError)
                    {
                        return resultadoDestinoAgencia.Errors;
                    }

                    destinoAgencia = resultadoDestinoAgencia.Value;

                    break;
                }

            case TipoAgrupacionEnvio.PorAlmacenDestino:
                {
                    var resultadoDestinosAlmacen =
                        await ObtenerDestinosAlmacenAsync(
                            request.Empresa,
                            expediciones,
                            cancellationToken);

                    if (resultadoDestinosAlmacen.IsError)
                    {
                        return resultadoDestinosAlmacen.Errors;
                    }

                    destinosAlmacen =
                        resultadoDestinosAlmacen.Value;

                    break;
                }

            default:
                return Error.Validation(
                    "Documento.TipoAgrupacionNoSoportado",
                    $"El tipo de agrupación '{tipoAgrupacion}' no está soportado.");
        }

        // De momento mantenemos el origen proporcionado por el ERP.
        var origen = nuevas[0].ToOrigen();

        // Obtener referencia documento
        var referencia = await _documentReferenceGenerator.GenerateAsync(
                request.Empresa,
                almacen.Codigo,
                DateTime.Now,
                cancellationToken);

        var resultadoDocumento =
            DocumentoDigitalTransporte.Generar(
                empresa: request.Empresa,
                referencia,
                almacenId: almacen.Id,
                agenciaId: agencia.Id,
                origen: origen,
                rangoFechas: rango,
                expediciones: expediciones,
                tipoAgrupacion: tipoAgrupacion,
                destinoAgencia: destinoAgencia,
                destinosAlmacen: destinosAlmacen,
                usuarioGeneracionId: _usuarioContexto.Current?.Id,
                fechaGeneracion: DateTimeOffset.UtcNow);

        if (resultadoDocumento.IsError)
        {
            return resultadoDocumento.Errors;
        }

        var documento = resultadoDocumento.Value;

        await _documentoRepository.AddAsync(
            documento,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return documento.Id;
    }


    private async Task<ErrorOr<(Almacen Almacen, Agencia Agencia)>>
      ObtenerConfiguracionAsync(
          GenerarDocumentoCommand request,
          CancellationToken cancellationToken)
    {
        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null ||
            almacen.Empresa != request.Empresa)
        {
            return Error.Validation(
                "Almacen.NoConfigurado",
                $"El almacén '{request.AlmacenId}' no existe " +
                $"para la empresa '{request.Empresa}'.");
        }

        var agencia = await _agenciaRepository.GetByIdAsync(
            request.AgenciaId,
            cancellationToken);

        if (agencia is null)
        {
            return Error.Validation(
                "Agencia.NoConfigurada",
                $"La agencia '{request.AgenciaId}' no existe.");
        }

        if (!agencia.Activa)
        {
            return Error.Validation(
                "Agencia.Inactiva",
                $"La agencia '{agencia.Codigo}' no está activa.");
        }

        var agenciaDisponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                almacen.Id,
                agencia.Id,
                cancellationToken);

        if (!agenciaDisponible)
        {
            return Error.Validation(
                "Almacen.AgenciaNoDisponible",
                $"La agencia '{agencia.Codigo}' no está disponible " +
                $"para el almacén '{almacen.Codigo}'.");
        }

        return (almacen, agencia);
    }

    private async Task<ErrorOr<IReadOnlyList<ExpedicionErpDto>>>
        ObtenerExpedicionesErpAsync(
            string empresa,
            Almacen almacen,
            Agencia agencia,
            RangoFechas rango,
            CancellationToken cancellationToken)
    {
        try
        {
            var expediciones =
                await _erpGateway.GetExpedicionesAsync(
                    empresa,
                    almacen.Codigo,
                    agencia.Codigo,
                    rango,
                    cancellationToken);

            if (expediciones.Count == 0)
            {
                return Error.NotFound(
                    "Documento.SinExpediciones",
                    "No existen expediciones en el ERP " +
                    "para el rango y agencia indicados.");
            }

            return ErrorOrFactory
                .From<IReadOnlyList<ExpedicionErpDto>>(
                    expediciones);
        }
        catch (EmpresaNoConfiguradaException ex)
        {
            return Error.Failure(
                "Empresa.ErpNoConfigurado",
                ex.Message);
        }
        catch (ErpGatewayException ex)
        {
            return ErpGatewayErrorMapper.ToError(ex);
        }
    }

    private async Task<ErrorOr<IReadOnlyList<ExpedicionErpDto>>>
        ObtenerExpedicionesNuevasAsync(
            string empresa,
            Almacen almacen,
            Agencia agencia,
            IReadOnlyList<ExpedicionErpDto> expedicionesErp,
            CancellationToken cancellationToken)
    {
        var erpIds = expedicionesErp
            .Select(e => e.Id)
            .ToList();

        var yaIncluidos =
            await _documentoRepository.ObtenerErpIdsIncluidosAsync(
                empresa,
                almacen.Id,
                agencia.Id,
                erpIds,
                cancellationToken);

        var nuevas = expedicionesErp
            .Where(e => !yaIncluidos.Contains(e.Id))
            .ToList();

        if (nuevas.Count == 0)
        {
            return Error.Conflict(
                "Documento.ExpedicionesYaIncluidas",
                "Todas las expediciones del rango ya están " +
                "incluidas en documentos existentes.");
        }

        return ErrorOrFactory
            .From<IReadOnlyList<ExpedicionErpDto>>(
                nuevas);
    }

    private async Task<ErrorOr<DestinoEnvio>>
     ObtenerDestinoAgenciaAsync(
         Almacen almacen,
         Agencia agencia,
         CancellationToken cancellationToken)
    {
        var relacion = await _almacenRepository.GetRelacionAgenciaAsync(
            almacen.Id,
            agencia.Id,
            cancellationToken);

        if (relacion?.AgenciaBaseId is not { } agenciaBaseId)
        {
            return Error.Validation(
                "Documento.AgenciaBaseAgenciaNoConfigurado",
                $"No está configurada la agencia base para el almacén " +
                $"'{almacen.Codigo}' y la agencia '{agencia.Codigo}'.");
        }

        var agenciaConBases = await _agenciaRepository.GetByIdConBasesAsync(
            agencia.Id,
            cancellationToken);

        if (agenciaConBases is null)
        {
            return Error.Validation(
                "Documento.AgenciaNoExiste",
                $"La agencia '{agencia.Codigo}' no existe.");
        }

        var agenciaBase = agenciaConBases.Bases
            .FirstOrDefault(b => b.Id == agenciaBaseId);

        if (agenciaBase is null)
        {
            return Error.Validation(
                "Documento.AgenciaBaseAgenciaNoExiste",
                $"La agencia base configurada para el almacén '{almacen.Codigo}' " +
                $"y la agencia '{agencia.Codigo}' no existe o no pertenece a esa agencia.");
        }

        if (!agenciaBase.Activo)
        {
            return Error.Validation(
                "Documento.AgenciaBaseAgenciaInactiva",
                $"La agencia base '{agenciaBase.Codigo}' no está activa.");
        }

        if (!agenciaBase.TieneDireccionCompleta)
        {
            return Error.Validation(
                "Documento.AgenciaBaseAgenciaSinDireccion",
                $"La agencia base '{agenciaBase.Codigo}' no tiene dirección completa " +
                $"para el almacén '{almacen.Codigo}' y la agencia '{agencia.Codigo}'.");
        }

        return new DestinoEnvio(
            agenciaBase.Codigo,
            agenciaBase.Nombre,
            agenciaBase.Direccion!,
            agenciaBase.CodigoPostal!,
            agenciaBase.Municipio!,
            agenciaBase.CodigoPaisIso!,
            agenciaBase.Movil?.Valor);
    }

    private async Task<
        ErrorOr<IReadOnlyDictionary<string, DestinoEnvio>>>
        ObtenerDestinosAlmacenAsync(
            string empresa,
            IReadOnlyCollection<Expedicion> expediciones,
            CancellationToken cancellationToken)
    {
        var codigosDestino = expediciones
            .Where(e =>
                !string.IsNullOrWhiteSpace(
                    e.Destino.AlmacenDestino))
            .Select(e =>
                e.Destino.AlmacenDestino!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (codigosDestino.Count == 0)
        {
            return ErrorOrFactory
                .From<IReadOnlyDictionary<string, DestinoEnvio>>(
                    new Dictionary<string, DestinoEnvio>(
                        StringComparer.OrdinalIgnoreCase));
        }

        var almacenesDestino =
            await _almacenRepository.ObtenerPorCodigosAsync(
                empresa,
                codigosDestino,
                cancellationToken);

        var destinos = almacenesDestino
            .ToDictionary(
                a => a.Codigo,
                a => CrearDestinoEnvio(a),
                StringComparer.OrdinalIgnoreCase);

        return ErrorOrFactory
            .From<IReadOnlyDictionary<string, DestinoEnvio>>(
                destinos);
    }

    private static DestinoEnvio CrearDestinoEnvio(Almacen almacen)
    {
        return new DestinoEnvio(
            almacen.Codigo,
            almacen.Nombre,
            almacen.Direccion,
            almacen.CodigoPostal,
            almacen.Ciudad,
            almacen.CodigoPaisIso,
            almacen.Telefono);
    }
}
