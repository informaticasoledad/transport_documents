using Dtd.Application.GatewayContracts;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ReenviarNotificacion;

internal sealed class ReenviarNotificacionCommandHandler
    : IRequestHandler<
        ReenviarNotificacionCommand,
        ErrorOr<Success>>
{
    private const int MaximoReenvios = 3;

    private readonly IDocumentoRepository _documentoRepository;
    private readonly IDocutenGateway _docutenGateway;
    private readonly IUnitOfWork _unitOfWork;

    public ReenviarNotificacionCommandHandler(
        IDocumentoRepository documentoRepository,
        IDocutenGateway docutenGateway,
        IUnitOfWork unitOfWork)
    {
        _documentoRepository = documentoRepository;
        _docutenGateway = docutenGateway;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<Success>> Handle(
        ReenviarNotificacionCommand request,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(
            request.DocumentoId,
            cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        var validacion = documento.ValidarPuedeReenviarNotificacion(
            request.EnvioId,
            MaximoReenvios);

        if (validacion.IsError)
        {
            return validacion.Errors;
        }

        var envio = documento.Envios
            .First(x => x.Id == request.EnvioId);

        var shipment = await _docutenGateway
            .ObtenerShipmentConPartiesAsync(
                envio.PlataformaEnvioId!,
                cancellationToken);

        if (EsEstadoNoReenviable(shipment.ShipmentStatus))
        {
            return Error.Conflict(
                "Documento.Envio.EstadoDocutenNoPermitido",
                $"El envío en Docuten está en estado " +
                $"'{shipment.ShipmentStatus}' y no permite reenviar notificaciones.");
        }

        var party = shipment.Parties
            .Where(p =>
                string.Equals(
                    p.SigningRole,
                    "signer",
                    StringComparison.OrdinalIgnoreCase))
            .Where(p =>
                !string.Equals(
                    p.Status,
                    "ended",
                    StringComparison.OrdinalIgnoreCase))
            .Where(p => p.NotificationDate.HasValue)
            .OrderBy(p => p.SignOrder)
            .FirstOrDefault();

        if (party is null)
        {
            return Error.Conflict(
                "Documento.Envio.SinNotificacionReenviable",
                "No existe ningún firmante pendiente con una " +
                "notificación previa que pueda reenviarse.");
        }

        if (string.IsNullOrWhiteSpace(party.PartyId))
        {
            return Error.Failure(
                "Documento.Envio.PartyIdNoDisponible",
                "Docuten no ha devuelto el identificador del firmante.");
        }

        await _docutenGateway.ReenviarNotificacionAsync(
            party.PartyId,
            cancellationToken);

        documento.RegistrarReenvioNotificacion(
            request.EnvioId,
            DateTimeOffset.UtcNow);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success;
    }

    private static bool EsEstadoNoReenviable(
        string shipmentStatus)
    {
        return string.Equals(
                   shipmentStatus,
                   EstadoDocuten.Error,
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   shipmentStatus,
                   EstadoDocuten.Delivered,
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   shipmentStatus,
                   EstadoDocuten.Completed,
                   StringComparison.OrdinalIgnoreCase)
               ||
               string.Equals(
                   shipmentStatus,
                   EstadoDocuten.Cancelled,
                   StringComparison.OrdinalIgnoreCase);
    }
}