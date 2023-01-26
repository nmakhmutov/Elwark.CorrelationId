using Elwark.CorrelationId.Abstractions;

namespace Elwark.CorrelationId.Http;

internal sealed class CorrelationIdHandler : DelegatingHandler
{
    private readonly ICorrelationContextAccessor _accessor;

    public CorrelationIdHandler(ICorrelationContextAccessor accessor) =>
        _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (_accessor.CorrelationContext is null)
            return base.SendAsync(request, ct);

        if (!request.Headers.Contains(_accessor.CorrelationContext.Header))
            request.Headers.Add(_accessor.CorrelationContext.Header, _accessor.CorrelationContext.CorrelationId);

        return base.SendAsync(request, ct);
    }
}
