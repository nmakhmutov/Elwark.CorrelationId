namespace Elwark.CorrelationId.Abstractions;

public sealed class CorrelationContext
{
    /// <summary>
    ///     The default correlation ID is used in cases where the correlation has not been set by the
    ///     <see cref="ICorrelationIdProvider" />.
    /// </summary>
    public const string DefaultCorrelationId = "Not set";

    /// <summary>
    ///     Create a <see cref="CorrelationContext" /> instance.
    /// </summary>
    /// <param name="correlationId">The correlation ID on the context.</param>
    /// <param name="header">The name of the header from which the Correlation ID was read/written.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="header" /> is null or empty.</exception>
    public CorrelationContext(string? correlationId, string header)
    {
        ArgumentException.ThrowIfNullOrEmpty(header, nameof(header));

        CorrelationId = correlationId ?? DefaultCorrelationId;
        Header = header;
    }

    /// <summary>
    ///     The Correlation ID which is applicable to the current request.
    /// </summary>
    public string CorrelationId { get; }

    /// <summary>
    ///     The name of the header from which the Correlation ID was read/written.
    /// </summary>
    public string Header { get; }
}
