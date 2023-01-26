namespace Elwark.CorrelationId.Abstractions;

public interface ICorrelationContextFactory : IDisposable
{
    /// <summary>
    ///     Creates a new <see cref="CorrelationContext" /> with the correlation ID set for the current request.
    /// </summary>
    /// <param name="correlationId">The correlation ID to set on the context.</param>
    /// ///
    /// <param name="header">The header used to hold the correlation ID.</param>
    /// <returns>A new instance of a <see cref="CorrelationContext" />.</returns>
    CorrelationContext Create(string correlationId, string header);
}
