using Dtd.Application.GatewayContracts;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarCallbacksPlataforma;

public sealed record ListarCallbacksPlataformaQuery(Guid? DocumentoId, int Limit = 50)
    : IRequest<ErrorOr<IReadOnlyList<DocutenCallbackLogEntry>>>;

internal sealed class ListarCallbacksPlataformaQueryHandler     
    : IRequestHandler<ListarCallbacksPlataformaQuery, ErrorOr<IReadOnlyList<DocutenCallbackLogEntry>>>
{
    private const int MaxLimit = 200;

    private readonly IDocutenCallbackLogRepository _callbackLogRepository;

    public ListarCallbacksPlataformaQueryHandler(IDocutenCallbackLogRepository callbackLogRepository)
    {
        _callbackLogRepository = callbackLogRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<DocutenCallbackLogEntry>>> Handle(
        ListarCallbacksPlataformaQuery request,
        CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, MaxLimit);
        var callbacks = await _callbackLogRepository.ListRecentAsync(
            request.DocumentoId,
            limit,
            cancellationToken);

        return callbacks.ToList();
    }
}
