using Dtd.Application.Security;
using Dtd.Domain.Ccs;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Ccs;

/// <summary>
/// Activa o desactiva un CC del catálogo. Un único comando para los dos verbos; los endpoints
/// <c>/activar</c> y <c>/desactivar</c> son wrappers que fijan <c>Activo</c> a true/false. Carga con
/// tracking para que el cambio de <c>Activo</c> se persista con SaveChanges.
/// </summary>
/// <returns>El <see cref="CcCatalogoDto"/> con el nuevo estado.</returns>
public sealed record CambiarEstadoCcCommand(string Empresa, Guid CcId, bool Activo)
    : IRequest<ErrorOr<CcCatalogoDto>>;

internal sealed class CambiarEstadoCcCommandHandler
    : IRequestHandler<CambiarEstadoCcCommand, ErrorOr<CcCatalogoDto>>
{
    private readonly ICcRepository _ccRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public CambiarEstadoCcCommandHandler(
        ICcRepository ccRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _ccRepository = ccRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<CcCatalogoDto>> Handle(CambiarEstadoCcCommand request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var cc = await _ccRepository.GetByIdAsync(request.CcId, cancellationToken);
        if (cc is null)
        {
            return Error.NotFound(
                "Cc.NoEncontrado",
                $"No existe el CC '{request.CcId}'.");
        }

        if (cc.Empresa != empresa)
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El CC '{request.CcId}' no pertenece a la empresa '{empresa}'.");
        }

        if (request.Activo)
        {
            cc.Activar();
        }
        else
        {
            cc.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CrearCcCommandHandler.ToDto(cc);
    }
}