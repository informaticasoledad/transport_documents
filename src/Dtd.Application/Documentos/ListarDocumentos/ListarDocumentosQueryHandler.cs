using Dtd.Application.Security;
using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarDocumentos;

internal sealed class ListarDocumentosQueryHandler : IRequestHandler<ListarDocumentosQuery, ErrorOr<IReadOnlyList<DocumentoDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarDocumentosQueryHandler(
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository,
        IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<DocumentoDto>>> Handle(ListarDocumentosQuery request, CancellationToken cancellationToken)
    {
        var estado = ParseEstado(request.Estado);
        if (request.Estado is not null && estado is null)
        {
            return Error.Validation("Documento.EstadoInvalido", $"Estado '{request.Estado}' no válido.");
        }

        // Filtro explícito de empresa: normalizar y, si hay usuario autenticado, verificar autorización.
        string? empresaFiltro = null;
        if (!string.IsNullOrWhiteSpace(request.Empresa))
        {
            empresaFiltro = request.Empresa.Trim();
            if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(empresaFiltro))
            {
                return Error.Forbidden(
                    "Empresa.NoAutorizada",
                    $"El usuario no tiene acceso a la empresa '{empresaFiltro}'.");
            }
        }

        // Sin empresa explícita y con usuario autenticado: restringir a las empresas del usuario.
        // Con Auth deshabilitada (Current == null) no se restringe nada (dev).
        IReadOnlyCollection<string>? empresasPermitidas = null;
        if (empresaFiltro is null && _usuarioContexto.Current is { } usuarioActual)
        {
            empresasPermitidas = usuarioActual.Empresas.Count > 0 ? usuarioActual.Empresas : null;
            // Si el usuario no tiene ninguna empresa autorizada, devolvemos lista vacía en lugar de
            // filtrar por un conjunto vacío (que algunas BBDD traducen a "todo" con Contains).
            if (empresasPermitidas is { Count: 0 })
            {
                return new List<DocumentoDto>();
            }
        }

        var filtro = new DocumentoFiltro(
            Empresa: empresaFiltro,
            Empresas: empresasPermitidas,
            AlmacenCodigo: request.AlmacenCodigo,
            AgenciaCodigo: request.AgenciaCodigo,
            FechaDesde: request.FechaDesde,
            FechaHasta: request.FechaHasta,
            Estado: estado);

        var documentos = await _documentoRepository.ListarAsync(filtro, cancellationToken);

        // El agregado sólo guarda los Ids (FK); el código/nombre del read model se resuelve en batch
        // desde los maestros locales (una consulta por maestro, no N+1). No filtra por Activo.
        var almacenIds = documentos.Select(d => d.AlmacenId).Distinct().ToList();
        var agenciaIds = documentos.Select(d => d.AgenciaId).Distinct().ToList();

        var almacenes = await _almacenRepository.GetByIdsAsync(almacenIds, cancellationToken);
        var agencias = await _agenciaRepository.GetByIdsAsync(agenciaIds, cancellationToken);

        var almacenPorId = almacenes.ToDictionary(a => a.Id);
        var agenciaPorId = agencias.ToDictionary(a => a.Id);
        return documentos
            .Select(d => DocumentoDtoFactory.ToDto(
                d,
                almacenPorId.TryGetValue(d.AlmacenId, out var a) ? a : null,
                agenciaPorId.TryGetValue(d.AgenciaId, out var g) ? g : null))
            .ToList();
    }

    private static EstadoDocumento? ParseEstado(string? estado) => estado switch
    {
        null => null,
        "" => null,
        _ => Enum.TryParse<EstadoDocumento>(estado, ignoreCase: true, out var e) ? e : null
    };
}
