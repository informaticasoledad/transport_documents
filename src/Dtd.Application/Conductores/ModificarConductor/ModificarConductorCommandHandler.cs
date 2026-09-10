using Dtd.Application.Conductores.CrearConductor;
using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ModificarConductor;

internal sealed class ModificarConductorCommandHandler
    : IRequestHandler<
        ModificarConductorCommand,
        ErrorOr<ConductorCatalogoDto>>
{
    private readonly IConductorRepository _conductorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ModificarConductorCommandHandler(
        IConductorRepository conductorRepository,
        IUnitOfWork unitOfWork)
    {
        _conductorRepository = conductorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ConductorCatalogoDto>> Handle(
      ModificarConductorCommand request,
      CancellationToken cancellationToken)
    {
        var conductor = await _conductorRepository.GetByIdAsync(
            request.ConductorId,
            cancellationToken);

        if (conductor is null)
        {
            return Error.NotFound(
                "Conductor.NoEncontrado",
                $"No existe el conductor '{request.ConductorId}'.");
        }

        var taxId = string.IsNullOrWhiteSpace(request.TaxId)
            ? null
            : request.TaxId.Trim().ToUpperInvariant();

        if (taxId is not null)
        {
            var existeOtro = await _conductorRepository.ExistsByTaxIdExceptIdAsync(
                taxId,
                request.ConductorId,
                cancellationToken);

            if (existeOtro)
            {
                return Error.Conflict(
                    "Conductor.TaxIdDuplicado",
                    $"Ya existe otro conductor con NIF '{taxId}'.");
            }
        }

        var movil = string.IsNullOrWhiteSpace(request.Movil)
            ? null
            : Movil.Create(request.Movil);

        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : Email.Create(request.Email);

        var canal = Canal.Create(request.Canal);

        conductor.Modificar(
            request.Nombre.Trim(),
            canal,
            movil,
            email,
            taxId,
            request.LicensePlate?.Trim(),
            request.Language);

        if (request.Activo)
        {
            conductor.Activar();
        }
        else
        {
            conductor.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearConductorCommandHandler.ToDto(conductor);
    }
}