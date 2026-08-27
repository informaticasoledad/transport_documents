using Dtd.Application.Almacenes;
using Dtd.Domain.Common;
using Dtd.Domain.AgenciaBases;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases;

/// <summary>
/// Activa o desactiva un agenciaBase del catálogo. Un único comando para los dos verbos; los endpoints
/// <c>/activar</c> y <c>/desactivar</c> son wrappers que fijan <c>Activo</c> a true/false. Carga con
/// tracking para que el cambio de <c>Activo</c> se persista con SaveChanges.
/// </summary>
/// <returns>El <see cref="AgenciaBaseCatalogoDto"/> con el nuevo estado.</returns>
public sealed record CambiarEstadoAgenciaBaseCommand(
    string Empresa,
    Guid AgenciaBaseId,
    bool Activo)
    : IRequest<ErrorOr<AgenciaBaseCatalogoDto>>;

internal sealed class CambiarEstadoAgenciaBaseCommandHandler
    : IRequestHandler<
        CambiarEstadoAgenciaBaseCommand,
        ErrorOr<AgenciaBaseCatalogoDto>>
{
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public CambiarEstadoAgenciaBaseCommandHandler(
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<AgenciaBaseCatalogoDto>> Handle(
        CambiarEstadoAgenciaBaseCommand request,
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

        var agenciaBase =
            await _agenciaBaseRepository.GetByIdAsync(
                request.AgenciaBaseId,
                cancellationToken);

        if (agenciaBase is null ||
            agenciaBase.Empresa != empresa)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrado",
                $"No existe el agenciaBase '{request.AgenciaBaseId}' " +
                $"para la empresa '{empresa}'.");
        }

        if (request.Activo)
        {
            agenciaBase.Activar();
        }
        else
        {
            agenciaBase.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearAgenciaBaseCommandHandler.ToDto(
            agenciaBase);
    }
}