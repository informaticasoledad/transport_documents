using Dtd.Domain.Common;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.CrearConductor;

internal sealed class CrearConductorCommandHandler
    : IRequestHandler<
        CrearConductorCommand,
        ErrorOr<ConductorCatalogoDto>>
{
    private readonly IConductorRepository _conductorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearConductorCommandHandler(
        IConductorRepository conductorRepository,
        IUnitOfWork unitOfWork)
    {
        _conductorRepository = conductorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ErrorOr<ConductorCatalogoDto>> Handle(
        CrearConductorCommand request,
        CancellationToken cancellationToken)
    {
        var movil = string.IsNullOrWhiteSpace(request.Movil)
            ? null
            : Movil.Create(request.Movil);

        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : Email.Create(request.Email);

        var canal = Canal.Create(request.Canal);

        var conductor = Conductor.Crear(
            request.Nombre.Trim(),
            canal,
            movil,
            email,
            request.TaxId?.Trim(),
            request.LicensePlate?.Trim(),
            request.Language);

        await _conductorRepository.AddAsync(
            conductor,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return ToDto(conductor);
    }

    internal static ConductorCatalogoDto ToDto(
        Conductor conductor)
    {
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