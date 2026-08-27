using Dtd.Application.Almacenes;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>
/// Activa o desactiva un CC del catálogo. Un único comando para los dos verbos; los endpoints
/// <c>/activar</c> y <c>/desactivar</c> son wrappers que fijan <c>Activo</c> a true/false.
/// </summary>
/// <returns>El <see cref="CcCatalogoDto"/> con el nuevo estado.</returns>
public sealed record CambiarEstadoCcCommand(
    string Empresa,
    Guid CcId,
    bool Activo)
    : IRequest<ErrorOr<CcCatalogoDto>>;

internal sealed class CambiarEstadoCcCommandHandler
    : IRequestHandler<CambiarEstadoCcCommand, ErrorOr<CcCatalogoDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public CambiarEstadoCcCommandHandler(
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<CcCatalogoDto>> Handle(
        CambiarEstadoCcCommand request,
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

        var cc = await _ccRepository.GetByIdAsync(
            request.CcId,
            cancellationToken);

        if (cc is null ||
            cc.Empresa != empresa)
        {
            return Error.NotFound(
                "Cc.NoEncontrado",
                $"No existe el CC '{request.CcId}' " +
                $"para la empresa '{empresa}'.");
        }

        if (request.Activo)
        {
            cc.Activar();
        }
        else
        {
            cc.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return CrearCcCommandHandler.ToDto(cc);
    }
}