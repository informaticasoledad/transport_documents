using Dtd.Application.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ConductoresDocumento;

internal sealed class AgregarConductorManualDocumentoCommandHandler
    : IRequestHandler<
        AgregarConductorManualDocumentoCommand,
        ErrorOr<ConductorDto>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;
    private readonly IUnitOfWork _unitOfWork;

    public AgregarConductorManualDocumentoCommandHandler(
        IDocumentoRepository documentoRepository,
        IAccesoAlmacenService accesoAlmacenService,
        IUnitOfWork unitOfWork)
    {
        _documentoRepository = documentoRepository;
        _accesoAlmacenService = accesoAlmacenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ConductorDto>> Handle(
        AgregarConductorManualDocumentoCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Recuperar documento
        var documento = await _documentoRepository.GetByIdAsync(
            request.DocumentoId,
            cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        // 2. Validar acceso al almacén
        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                documento.Empresa,
                documento.AlmacenId,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        // 3. Crear Value Object del móvil
        Movil movil;

        try
        {
            movil = Movil.Create(request.Movil)
                ?? throw new ArgumentException(
                    "El móvil del conductor es obligatorio.");
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Conductor.MovilInvalido",
                ex.Message);
        }

        // 4. Crear y asignar conductor manual
        ConductorAsignado conductor;

        try
        {
            conductor = ConductorAsignado.CrearManual(
                request.Nombre,
                request.TaxId,
                movil);

            documento.AsignarConductor(conductor);
        }
        catch (ArgumentException ex)
        {
            return Error.Validation(
                "Conductor.DatosInvalidos",
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Error.Conflict(
                "Documento.YaConfirmado",
                ex.Message);
        }

        // 5. Persistir
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        // 6. Devolver el conductor asignado
        return new ConductorDto
        {
            Id = conductor.Id,
            Nombre = conductor.Nombre,
            TaxId = conductor.TaxId,
            LicensePlate = conductor.LicensePlate,
            Channel = conductor.Canal.Valor,
            Email = conductor.Email?.Valor,
            Movil = conductor.Movil?.Valor,
            Language = conductor.Language
        };
    }
}