using Dtd.Application.Almacenes;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Ccs;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Dtd.Application.Ccs.ObtenerCcsDefecto;

public sealed record ObtenerCcsDefectoQuery(
    string Empresa,
    Guid AlmacenId,
    Guid AgenciaId)
    : IRequest<ErrorOr<IReadOnlyList<CcCatalogoDto>>>;

internal sealed class ObtenerCcsDefectoQueryValidator
    : AbstractValidator<ObtenerCcsDefectoQuery>
{
    public ObtenerCcsDefectoQueryValidator()
    {
        RuleFor(x => x.Empresa)
            .NotEmpty();

        RuleFor(x => x.AlmacenId)
            .NotEmpty();

        RuleFor(x => x.AgenciaId)
            .NotEmpty();
    }
}

internal sealed class ObtenerCcsDefectoQueryHandler
    : IRequestHandler<
        ObtenerCcsDefectoQuery,
        ErrorOr<IReadOnlyList<CcCatalogoDto>>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ObtenerCcsDefectoQueryHandler(
        ICcRepository ccRepository,
        IAlmacenRepository almacenRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
        _almacenRepository = almacenRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<CcCatalogoDto>>> Handle(
        ObtenerCcsDefectoQuery request,
        CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        var accesoEmpresa =
            await _accesoAlmacenService.ValidarAccesoEmpresaAsync(
                empresa,
                cancellationToken);

        if (accesoEmpresa.IsError)
        {
            return accesoEmpresa.Errors;
        }

        var almacen = await _almacenRepository.GetByIdAsync(
            request.AlmacenId,
            cancellationToken);

        if (almacen is null ||
            almacen.Empresa != empresa)
        {
            return Error.NotFound(
                "Cc.AlmacenNoExiste",
                $"El almacén '{request.AlmacenId}' no existe " +
                $"para la empresa '{empresa}'.");
        }

        var disponible =
            await _almacenRepository.EsAgenciaDisponibleAsync(
                request.AlmacenId,
                request.AgenciaId,
                cancellationToken);

        if (!disponible)
        {
            return Error.NotFound(
                "Cc.AlmacenAgenciaNoDisponible",
                "La relación almacén-agencia indicada no existe.");
        }

        var ccs = await _ccRepository.ObtenerCcsDefectoAsync(
            request.AlmacenId,
            request.AgenciaId,
            cancellationToken);

        return ccs
            .Select(CrearCcCommandHandler.ToDto)
            .ToList();
    }
}