using System.Net.Http;
using Dtd.Application.Almacenes;
using Dtd.Application.GatewayContracts;
using Dtd.Application.Mapping;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.EnviarDocumentoADocuten;

internal sealed class EnviarDocumentoADocutenCommandHandler
    : IRequestHandler<
        EnviarDocumentoADocutenCommand,
        ErrorOr<DocumentoEnviadoDto>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocutenGateway _docutenGateway;
    private readonly IEmpresaResolver _empresaResolver;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly DocutenMappingOptions _docutenMappingOptions;
    private readonly IDocutenDocumentoProvider _docutenDocumentoProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public EnviarDocumentoADocutenCommandHandler(
        IDocumentoRepository documentoRepository,
        IDocutenGateway docutenGateway,
        IEmpresaResolver empresaResolver,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        DocutenMappingOptions docutenMappingOptions,
        IDocutenDocumentoProvider docutenDocumentoProvider,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _docutenGateway = docutenGateway;
        _empresaResolver = empresaResolver;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _docutenMappingOptions = docutenMappingOptions;
        _docutenDocumentoProvider = docutenDocumentoProvider;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<DocumentoEnviadoDto>> Handle(
        EnviarDocumentoADocutenCommand request,
        CancellationToken cancellationToken)
    {
        var documento =
            await _documentoRepository.GetByIdAsync(
                request.DocumentoId,
                cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                documento.Empresa,
                documento.AlmacenId,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        // Reglas de "listo para enviar" (única fuente de verdad en el agregado):
        // estado Nuevo, al menos una expedición, al menos un conductor
        // y canal coherente en todos.
        var validacion = documento.ValidarListoParaEnviar();

        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        var empresaConfig =
            await _empresaResolver.ResolveAsync(
                documento.Empresa,
                cancellationToken);

        if (empresaConfig is null)
        {
            return Error.Failure(
                "Empresa.ErpNoConfigurado",
                $"La empresa '{documento.Empresa}' no tiene configuración " +
                "(tabla empresas). Sin ella no se puede construir el lote " +
                "de Docuten (falta consignor/tax_id).");
        }

        var almacen =
            await _almacenRepository.GetByIdAsync(
                documento.AlmacenId,
                cancellationToken);

        if (almacen is null ||
            almacen.Empresa != documento.Empresa)
        {
            return Error.Failure(
                "Almacen.NoConfigurado",
                $"El almacén '{documento.AlmacenId}' de la empresa " +
                $"'{documento.Empresa}' no existe en la tabla almacenes. " +
                "Sin él no se puede construir el consignor del lote de Docuten.");
        }

        var agencia =
            await _agenciaRepository.GetByIdAsync(
                documento.AgenciaId,
                cancellationToken);

        if (agencia is null ||
            agencia.Empresa != documento.Empresa)
        {
            return Error.Failure(
                "Agencia.NoEncontrada",
                $"La agencia '{documento.AgenciaId}' de la empresa " +
                $"'{documento.Empresa}' no existe en el catálogo. " +
                "Sin ella no se puede construir el carrier del lote de Docuten.");
        }

        var almacenAgencia =
            await _almacenRepository.GetRelacionAgenciaAsync(
                documento.AlmacenId,
                documento.AgenciaId,
                cancellationToken);

        if (almacenAgencia is null)
        {
            return Error.Failure(
                "AlmacenAgencia.NoConfigurado",
                $"No existe configuración para el almacén " +
                $"'{documento.AlmacenId}' y la agencia " +
                $"'{documento.AgenciaId}'.");
        }

        if (almacenAgencia.Template is null)
        {
            return Error.Failure(
                "Template.NoConfigurado",
                $"No hay ninguna plantilla configurada para el almacén " +
                $"'{documento.AlmacenId}' y la agencia " +
                $"'{documento.AgenciaId}'.");
        }

        if (!almacenAgencia.Template.Active)
        {
            return Error.Failure(
                "Template.NoActivo",
                $"La plantilla '{almacenAgencia.Template.Code}' no está activa.");
        }

        var lote = await documento.ToDocutenLoteDto(
            empresaConfig,
            almacen,
            agencia,
            almacenAgencia.Template,
            _docutenMappingOptions,
            _docutenDocumentoProvider,
            cancellationToken);

        DocutenLoteEnvioResult envio;

        try
        {
            envio = await _docutenGateway.EnviarAsync(
                lote,
                cancellationToken);
        }
        catch (Exception ex)
        {
            var estadoHttp = ex is HttpRequestException hre
                ? (int?)hre.StatusCode
                : null;

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return Error.Failure(
                "Documento.EnvioFallido",
                $"No se pudo transmitir el documento a Docuten: {ex.Message}. " +
                "Queda registrado el intento y el documento se puede reintentar.");
        }

        documento.ConfirmarEnvioADocuten(
            envio.LotId,
            envio.Estado);

        foreach (var shipment in envio.Shipments)
        {
            documento.ConfirmarEnvioPlataforma(
                shipment.ShipmentReference ?? string.Empty,
                shipment.ShipmentId,
                shipment.ShipmentStatus);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new DocumentoEnviadoDto(
            documento.Id,
            envio.LotId,
            envio.Estado);
    }
}