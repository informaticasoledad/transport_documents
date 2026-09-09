using Dtd.Application.Almacenes;
using Dtd.Application.Documentos;
using Dtd.Domain.Documentos;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarEventosDocumento;

public sealed record ListarEventosDocumentoQuery(
    Guid DocumentoId)
    : IRequest<ErrorOr<IReadOnlyList<DocumentoEventoDto>>>;

internal sealed class ListarEventosDocumentoQueryHandler
    : IRequestHandler<
        ListarEventosDocumentoQuery,
        ErrorOr<IReadOnlyList<DocumentoEventoDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IAccesoAlmacenService _accesoAlmacenService;

    public ListarEventosDocumentoQueryHandler(
        IDocumentoRepository documentoRepository,
        IAccesoAlmacenService accesoAlmacenService)
    {
        _documentoRepository = documentoRepository;
        _accesoAlmacenService = accesoAlmacenService;
    }

    public async Task<ErrorOr<IReadOnlyList<DocumentoEventoDto>>> Handle(
        ListarEventosDocumentoQuery request,
        CancellationToken cancellationToken)
    {
        var documento =
            await _documentoRepository.GetByIdAsync(
                request.DocumentoId,
                cancellationToken);

        if (documento is null)
        {
            return Error.NotFound(
                "Documento.NoEncontrado",
                $"No existe el documento '{request.DocumentoId}'.");
        }

        var accesoAlmacen =
            await _accesoAlmacenService.ValidarAccesoAsync(
                documento.Empresa,
                documento.AlmacenId,
                cancellationToken);

        if (accesoAlmacen.IsError)
        {
            return accesoAlmacen.Errors;
        }

        var eventos =
            await _documentoRepository.GetEventosAsync(
                request.DocumentoId,
                cancellationToken);

        return eventos
            .Select(x => new DocumentoEventoDto
            {
                Id = x.Id,
                Fecha = x.Fecha,
                Tipo = x.Tipo.ToString(),
                EstadoAnterior = x.EstadoAnterior?.ToString(),
                EstadoNuevo = x.EstadoNuevo?.ToString(),
                Descripcion = x.Descripcion,
                Origen = x.Origen,
                Usuario = x.Usuario,
                EnvioId = x.EnvioId
            })
            .ToList();
    }
}