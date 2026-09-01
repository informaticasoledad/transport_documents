using Dtd.Domain.Agencias;
using Dtd.Domain.Almacenes;
using Dtd.Domain.Documentos;
using Dtd.Domain.Documentos.ValueObjects;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarDocumentos;

internal sealed class ListarDocumentosQueryHandler
    : IRequestHandler<ListarDocumentosQuery, ErrorOr<IReadOnlyList<DocumentoDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAlmacenRepository _almacenRepository;
    private readonly IAgenciaRepository _agenciaRepository;

    public ListarDocumentosQueryHandler(
        IDocumentoRepository documentoRepository,
        IAlmacenRepository almacenRepository,
        IAgenciaRepository agenciaRepository)
    {
        _documentoRepository = documentoRepository;
        _almacenRepository = almacenRepository;
        _agenciaRepository = agenciaRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<DocumentoDto>>> Handle(
        ListarDocumentosQuery request,
        CancellationToken cancellationToken)
    {
        var estado = ParseEstado(request.Estado);

        if (!string.IsNullOrWhiteSpace(request.Estado) && estado is null)
        {
            return Error.Validation(
                "Documento.EstadoInvalido",
                $"Estado '{request.Estado}' no válido.");
        }

        var empresaFiltro = request.Empresa.Trim();

        var filtro = new DocumentoFiltro(
            Empresa: empresaFiltro,
            AlmacenId: request.AlmacenId,
            AgenciaId: request.AgenciaId,
            FechaDesde: request.FechaDesde,
            FechaHasta: request.FechaHasta,
            Estado: estado,
            Finalizado: request.Finalizado);

        var documentos = await _documentoRepository.ListarAsync(
            filtro,
            cancellationToken);

        var almacenIds = documentos
            .Select(d => d.AlmacenId)
            .Distinct()
            .ToList();

        var agenciaIds = documentos
            .Select(d => d.AgenciaId)
            .Distinct()
            .ToList();

        var almacenes = await _almacenRepository.GetByIdsAsync(
            almacenIds,
            cancellationToken);

        var agencias = await _agenciaRepository.GetByIdsAsync(
            agenciaIds,
            cancellationToken);

        var almacenPorId = almacenes.ToDictionary(a => a.Id);
        var agenciaPorId = agencias.ToDictionary(a => a.Id);

        return documentos
            .Select(d => DocumentoDtoFactory.ToDto(
                d,
                almacenPorId.TryGetValue(d.AlmacenId, out var almacen)
                    ? almacen
                    : null,
                agenciaPorId.TryGetValue(d.AgenciaId, out var agencia)
                    ? agencia
                    : null))
            .ToList();
    }

    private static EstadoDocumento? ParseEstado(string? estado) =>
        estado switch
        {
            null => null,
            "" => null,
            _ => Enum.TryParse<EstadoDocumento>(
                estado,
                ignoreCase: true,
                out var parsed)
                    ? parsed
                    : null
        };
}