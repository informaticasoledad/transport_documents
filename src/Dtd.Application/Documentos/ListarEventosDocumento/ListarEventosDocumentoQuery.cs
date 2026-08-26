using Dtd.Application.Documentos;
using Dtd.Application.Security;
using Dtd.Domain.Common;
using Dtd.Domain.Documentos;
using ErrorOr;
using Mapster;
using MediatR;

namespace Dtd.Application.Documentos.ListarEventosDocumento;

public sealed record ListarEventosDocumentoQuery(Guid DocumentoId) : IRequest<ErrorOr<IReadOnlyList<EventoDocumentoDto>>>;

internal sealed class ListarEventosDocumentoQueryHandler : IRequestHandler<ListarEventosDocumentoQuery, ErrorOr<IReadOnlyList<EventoDocumentoDto>>>
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly IUsuarioContexto _usuarioContexto;

    public ListarEventosDocumentoQueryHandler(IDocumentoRepository documentoRepository, IUsuarioContexto usuarioContexto)
    {
        _documentoRepository = documentoRepository;
        _usuarioContexto = usuarioContexto;
    }

    public async Task<ErrorOr<IReadOnlyList<EventoDocumentoDto>>> Handle(ListarEventosDocumentoQuery request, CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(request.DocumentoId, cancellationToken);
        if (documento is null)
        {
            return Error.NotFound("Documento.NoEncontrado", $"No existe el documento '{request.DocumentoId}'.");
        }

        // Autorización por empresa: el usuario debe tener acceso a la empresa del documento.
        if (_usuarioContexto.Current is { } usuario && !usuario.Empresas.Contains(documento.Empresa))
        {
            return Error.Forbidden(
                "Empresa.NoAutorizada",
                $"El usuario no tiene acceso a la empresa '{documento.Empresa}'.");
        }

        //ojo revisar 
        return new List<EventoDocumentoDto>();
    }
}