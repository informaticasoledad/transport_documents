using System.Net.Http;
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

namespace Dtd.Application.Documentos.EnviarDocumentoADocuten;

internal sealed class EnviarDocumentoADocutenCommandHandler : IRequestHandler<EnviarDocumentoADocutenCommand, ErrorOr<DocumentoEnviadoDto>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocutenGateway _docutenGateway;
    private readonly IEmpresaResolver _empresaResolver;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly DocutenMappingOptions _docutenMappingOptions;
    private readonly IDocutenDocumentoProvider _docutenDocumentoProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public EnviarDocumentoADocutenCommandHandler(
        IDocumentoRepository documentoRepository,
        IDocutenGateway docutenGateway,
        IEmpresaResolver empresaResolver,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        DocutenMappingOptions docutenMappingOptions,
        IDocutenDocumentoProvider docutenDocumentoProvider,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _docutenGateway = docutenGateway;
        _empresaResolver = empresaResolver;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _docutenMappingOptions = docutenMappingOptions;
        _docutenDocumentoProvider = docutenDocumentoProvider;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<DocumentoEnviadoDto>> Handle(EnviarDocumentoADocutenCommand request, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(request.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return Error.NotFound("Documento.NoEncontrado", $"No existe el documento '{request.DocumentoId}'.");
        }

        // Autorización por empresa: el documento pertenece a una empresa; el usuario debe tener acceso.
        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(documento.Empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{documento.Empresa}'.");
        }

        // Reglas de "listo para enviar" (única fuente de verdad en el agregado: ValidarListoParaEnviar):
        // estado Nuevo, al menos una expedición, al menos un conductor y canal coherente en todos.
        // Se valida antes de cualquier I/O (empresa/almacén) para fallar rápido y no transmitir a
        // Docuten un lote que el dominio ya sabe que es inválido (p.ej. sin expediciones).
        var validacion = documento.ValidarListoParaEnviar();
        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        // El consignor del lote sale de la empresa (tax_id/nombre); sin fila de empresa no hay lote válido.
        var empresaConfig = await _empresaResolver.ResolveAsync(documento.Empresa, cancellationToken);
        if (empresaConfig is null)
        {
            return Error.Failure(
                "Empresa.ErpNoConfigurado",
                $"La empresa '{documento.Empresa}' no tiene configuración (tabla empresas). " +
                "Sin ella no se puede construir el lote de Docuten (falta consignor/tax_id).");
        }

        // El consignor del lote necesita la dirección y el contacto del almacén (delegación que carga),
        // leído de la tabla local `almacenes` por Id en el momento del envío (defense-in-depth: ya se
        // validó al generar, pero el almacén podría haberse borrado entre medias). Sin almacén no hay
        // consignor. La agencia se carga por Id para el carrier del lote (metadata + LotName).
        var almacen = await _almacenRepository.GetByIdAsync(documento.AlmacenId, cancellationToken);
        if (almacen is null)
        {
            return Error.Failure(
                "Almacen.NoConfigurado",
                $"El almacén '{documento.AlmacenId}' de la empresa '{documento.Empresa}' no existe en la tabla almacenes. " +
                "Sin él no se puede construir el consignor del lote de Docuten (falta dirección/contacto).");
        }

        var agencia = await _agenciaRepository.GetByIdAsync(documento.AgenciaId, cancellationToken);
        if (agencia is null)
        {
            return Error.Failure(
                "Agencia.NoEncontrada",
                $"La agencia '{documento.AgenciaId}' de la empresa '{documento.Empresa}' no existe en el catálogo. " +
                "Sin ella no se puede construir el carrier del lote de Docuten.");
        }

        var lote = await documento.ToDocutenLoteDto(
            empresaConfig,
            almacen,
            agencia,
            _docutenMappingOptions,
            _docutenDocumentoProvider,
            cancellationToken);

        // Only the transmission to Docuten belongs to the retry path: while the document is still
        // Nuevo, a failure here is recorded in the retry log and the document stays retryable.
        DocutenLoteEnvioResult envio;
        try
        {
            envio = await _docutenGateway.EnviarAsync(lote, cancellationToken);
        }
        catch (Exception ex)
        {
            // Transient HTTP retries are handled by AddStandardResilienceHandler; if it still fails,
            // record the attempt in the retry log and keep the document in Nuevo so it can be resent.
            var estadoHttp = ex is HttpRequestException hre ? (int?)hre.StatusCode : null;
      
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Error.Failure(
                "Documento.EnvioFallido",
                $"No se pudo transmitir el documento a Docuten: {ex.Message}. Queda registrado el intento y el documento se puede reintentar.");
        }

        // Transmission succeeded: record the successful attempt and advance the pipeline to Enviando.
        // A persistence failure here is not a transmission retry, so it propagates (no fallido recorded).
        documento.ConfirmarEnvioADocuten(envio.LotId, envio.Estado);
        foreach (var shipment in envio.Shipments)
        {
            documento.ConfirmarEnvioPlataforma(
                shipment.ShipmentReference ?? string.Empty,
                shipment.ShipmentId,
                shipment.ShipmentStatus);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DocumentoEnviadoDto(documento.Id, envio.LotId, envio.Estado);
    }
}
