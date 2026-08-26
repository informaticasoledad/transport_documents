using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.AgenciaBases;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.AgenciaBases;

/// <summary>
/// Activa o desactiva un agenciaBase del catálogo. Un único comando para los dos verbos; los endpoints
/// <c>/activar</c> y <c>/desactivar</c> son wrappers que fijan <c>Activo</c> a true/false. Carga con
/// tracking para que el cambio de <c>Activo</c> se persista con SaveChanges.
/// </summary>
/// <returns>El <see cref="AgenciaBaseCatalogoDto"/> con el nuevo estado.</returns>
public sealed record CambiarEstadoAgenciaBaseCommand(string Empresa, Guid AgenciaBaseId, bool Activo)
    : IRequest<ErrorOr<AgenciaBaseCatalogoDto>>;

internal sealed class CambiarEstadoAgenciaBaseCommandHandler
    : IRequestHandler<CambiarEstadoAgenciaBaseCommand, ErrorOr<AgenciaBaseCatalogoDto>>
{
    private readonly IAgenciaBaseRepository _agenciaBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsuarioContexto _usuarioContexto;

    public CambiarEstadoAgenciaBaseCommandHandler(
        IAgenciaBaseRepository agenciaBaseRepository,
        IUnitOfWork unitOfWork,
        IUsuarioContexto usuarioContexto)
    {
        _agenciaBaseRepository = agenciaBaseRepository;
        _unitOfWork = unitOfWork;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<AgenciaBaseCatalogoDto>> Handle(CambiarEstadoAgenciaBaseCommand request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agenciaBase = await _agenciaBaseRepository.GetByIdAsync(request.AgenciaBaseId, cancellationToken);
        if (agenciaBase is null)
        {
            return Error.NotFound(
                "AgenciaBase.NoEncontrado",
                $"No existe el agenciaBase '{request.AgenciaBaseId}'.");
        }

        if (agenciaBase.Empresa != empresa)
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El agenciaBase '{request.AgenciaBaseId}' no pertenece a la empresa '{empresa}'.");
        }

        if (request.Activo)
        {
            agenciaBase.Activar();
        }
        else
        {
            agenciaBase.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CrearAgenciaBaseCommandHandler.ToDto(agenciaBase);
    }
}