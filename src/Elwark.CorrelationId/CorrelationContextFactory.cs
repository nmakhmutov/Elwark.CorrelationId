using Elwark.CorrelationId.Abstractions;

namespace Elwark.CorrelationId;

internal sealed class CorrelationContextFactory : ICorrelationContextFactory
{
    private readonly ICorrelationContextAccessor _accessor;

    public CorrelationContextFactory(ICorrelationContextAccessor accessor) =>
        _accessor = accessor;

    public CorrelationContext Create(string correlationId, string header) =>
        _accessor.CorrelationContext = new CorrelationContext(correlationId, header);

    public void Dispose() =>
        _accessor.CorrelationContext = null;
}
