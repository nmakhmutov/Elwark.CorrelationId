using Elwark.CorrelationId.Abstractions;

namespace Elwark.CorrelationId;

internal sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> Context = new();

    public CorrelationContext? CorrelationContext
    {
        get => Context.Value;
        set => Context.Value = value;
    }
}
