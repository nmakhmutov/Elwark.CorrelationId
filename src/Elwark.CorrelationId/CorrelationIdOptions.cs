namespace Elwark.CorrelationId;

public sealed class CorrelationIdOptions
{
    /// <summary>
    ///     The default header used for correlation ID.
    /// </summary>
    public const string DefaultHeader = "X-Correlation-Id";

    /// <summary>
    ///     The default logger scope key for correlation ID logging.
    /// </summary>
    public const string LoggerScopeKey = "CorrelationId";

    /// <summary>
    ///     The name of the header from which the Correlation ID is read from the request.
    /// </summary>
    public string RequestHeader { get; set; } = DefaultHeader;

    /// <summary>
    ///     The name of the header to which the Correlation ID is written for the response.
    /// </summary>
    public string ResponseHeader { get; set; } = DefaultHeader;

    /// <summary>
    ///     <para>
    ///         Ignore request header.
    ///         When true, the correlation ID for the current request ignores the correlation ID header value on the request.
    ///     </para>
    ///     <para>Default: false</para>
    /// </summary>
    public bool IgnoreRequestHeader { get; set; } = false;

    /// <summary>
    ///     <para>
    ///         Enforce the inclusion of the correlation ID request header.
    ///         When true and a correlation ID header is not included, the request will fail with a 400 Bad Request response.
    ///     </para>
    ///     <para>Default: false</para>
    /// </summary>
    public bool EnforceHeader { get; set; } = false;

    /// <summary>
    ///     <para>
    ///         Add the correlation ID value to the logger scope for all requests.
    ///         When true the value of the correlation ID will be added to the logger scope payload.
    ///     </para>
    ///     <para>Default: true</para>
    /// </summary>
    public bool AddToLoggingScope { get; set; } = true;

    /// <summary>
    ///     <para>
    ///         The name for the key used when adding the correlation ID to the logger scope.
    ///     </para>
    ///     <para>Default: 'CorrelationId'</para>
    /// </summary>
    public string LoggingScopeKey { get; set; } = LoggerScopeKey;

    /// <summary>
    ///     <para>
    ///         Controls whether the correlation ID is returned in the response headers.
    ///     </para>
    ///     <para>Default: true</para>
    /// </summary>
    public bool IncludeInResponse { get; set; } = true;

    /// <summary>
    ///     <para>
    ///         Controls whether the ASP.NET Core TraceIdentifier will be set to match the CorrelationId.
    ///     </para>
    ///     <para>Default: false</para>
    /// </summary>
    public bool UpdateTraceIdentifier { get; set; } = false;
}
