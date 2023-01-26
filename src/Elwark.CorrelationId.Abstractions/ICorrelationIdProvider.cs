namespace Elwark.CorrelationId.Abstractions;

public interface ICorrelationIdProvider
{
    /// <summary>
    ///     Generates a correlation ID string for the current request.
    /// </summary>
    /// <returns>A string representing the correlation ID.</returns>
    string GenerateCorrelationId();
}
