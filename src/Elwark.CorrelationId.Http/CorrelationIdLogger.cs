using Microsoft.Extensions.Logging;

namespace Elwark.CorrelationId.Http;

internal static partial class CorrelationIdLogger
{
    [LoggerMessage(1, LogLevel.Error, "The '{Header}' request header is required, but was not found")]
    internal static partial void RequestHeaderNotFound(this ILogger logger, string header);
}
