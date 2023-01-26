namespace Elwark.CorrelationId.Abstractions;

public interface ICorrelationContextAccessor
{
    /// <summary>
    ///     The <see cref="CorrelationContext" /> for the current request.
    /// </summary>
    CorrelationContext? CorrelationContext { get; set; }
}
