using Elwark.CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Elwark.CorrelationId.Http.Providers;

internal sealed class TraceIdCorrelationIdProvider : ICorrelationIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TraceIdCorrelationIdProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public string GenerateCorrelationId() =>
        _httpContextAccessor.HttpContext.TraceIdentifier;
}
