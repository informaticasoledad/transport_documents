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

        var movil = Movil.Create(request.Movil);
        var email = Email.Create(request.Email);
        var canal = Canal.Create(request.Canal);

        conductor.Modificar(
            request.Nombre,
            canal,
            movil,
            email,
            request.TaxId,
            request.LicensePlate,
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

        return new ConductorCatalogoDto
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