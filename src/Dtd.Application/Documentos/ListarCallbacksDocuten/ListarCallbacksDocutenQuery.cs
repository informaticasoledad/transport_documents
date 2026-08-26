using Dtd.Application.GatewayContracts;
using ErrorOr;
using MediatR;

namespace Dtd.Application.Documentos.ListarCallbacksDocuten;

public sealed record ListarCallbacksDocutenQuery(Guid? DocumentoId, int Limit = 50)
    : IRequest<ErrorOr<IReadOnlyList<DocutenCallbackLogEntry>>>;

internal sealed class ListarCallbacksDocutenQueryHandler
    : IRequestHandler<ListarCallbacksDocutenQuery, ErrorOr<IReadOnlyList<DocutenCallbackLogEntry>>>
{
    private const int MaxLimit = 200;

    private readonly IDocutenCallbackLogRepository _callbackLogRepository;

    public ListarCallbacksDocutenQueryHandler(IDocutenCallbackLogRepository callbackLogRepository)
    {
        _callbackLogRepository = callbackLogRepository;
    }

    public async Task<ErrorOr<IReadOnlyList<DocutenCallbackLogEntry>>> Handle(
        ListarCallbacksDocutenQuery request,
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
