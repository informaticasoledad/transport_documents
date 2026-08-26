using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Conductores;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Conductores.ListarConductores;

/// <summary>Lista los conductores activos del catálogo de una agencia de una empresa, para el dropdown
/// de selección del front al asignar conductores a un documento. La agencia se resuelve por
/// (empresa, agenciaCodigo) — el catálogo <c>conductores</c> cuelga 1:N de <c>agencias</c>.</summary>
public sealed record ListarConductoresQuery(string Empresa, string AgenciaCodigo)
    : IRequest<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>;

internal sealed class ListarConductoresQueryHandler
    : IRequestHandler<ListarConductoresQuery, ErrorOr<IReadOnlyList<ConductorCatalogoDto>>>
{
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IConductorRepository _conductorRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarConductoresQueryHandler(
        IAgenciaRepository agenciaRepository,
        IConductorRepository conductorRepository,
        IUsuarioContexto usuarioContexto)
    {
        _agenciaRepository = agenciaRepository;
        _conductorRepository = conductorRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<ConductorCatalogoDto>>> Handle(
        ListarConductoresQuery request, CancellationToken cancellationToken)
    {
        var empresa = request.Empresa.Trim();

        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{empresa}'.");
        }

        var agencia = await _agenciaRepository.GetByEmpresaYCodigoAsync(empresa, request.AgenciaCodigo, cancellationToken);
        if (agencia is null)
        {
            return Error.NotFound(
                "Agencia.NoEncontrada",
                $"La agencia '{request.AgenciaCodigo}' de la empresa '{empresa}' no existe en el catálogo.");
        }

        var conductores = await _conductorRepository.ListarPorAgenciaAsync(agencia.Id, cancellationToken);
        return conductores
            .Select(c => new ConductorCatalogoDto
            {
                Id = c.Id,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                TaxId = c.TaxId,
                LicensePlate = c.LicensePlate,
                Channel = c.Canal.Valor,
                Email = c.Email?.Valor,
                Movil = c.Movil?.Valor,
                Language = c.Language
            })
            .ToList();
    }
}