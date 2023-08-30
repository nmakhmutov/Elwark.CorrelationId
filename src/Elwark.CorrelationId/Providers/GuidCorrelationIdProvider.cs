using Elwark.CorrelationId.Abstractions;

namespace Elwark.CorrelationId.Providers;

internal sealed class GuidCorrelationIdProvider : ICorrelationIdProvider
{
    public string GenerateCorrelationId() =>
        Guid.NewGuid().ToString();
}
