using Microsoft.Extensions.Logging;

namespace Elwark.CorrelationId.Grpc;

internal static partial class CorrelationIdLogger
{
    [LoggerMessage(1, LogLevel.Error, "The '{Header}' request header is required, but was not found")]
    internal static partial void RequestHeaderNotFound(this ILogger logger, string header);

    [LoggerMessage(2, LogLevel.Debug, "Correlation id {Id} found in grpc metadata")]
    internal static partial void CorrelationIdFound(this ILogger logger, string id);
}
