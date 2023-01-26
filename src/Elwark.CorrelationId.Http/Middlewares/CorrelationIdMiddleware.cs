using Elwark.CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Elwark.CorrelationId.Http.Middlewares;

internal sealed class CorrelationIdMiddleware
{
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly CorrelationIdOptions _options;
    private readonly ICorrelationIdProvider _provider;

    public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger, RequestDelegate next,
        IOptions<CorrelationIdOptions> options, ICorrelationIdProvider provider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public async Task Invoke(HttpContext context, ICorrelationContextFactory factory)
    {
        var hasCorrelationId = TryGetIdFromHeader(context, out var header);
        if (_options.EnforceHeader && !hasCorrelationId)
        {
            await WriteHeaderNotFoundError(context);
            return;
        }

        var correlationId = GetOrCreateId(header);
        factory.Create(correlationId, _options.RequestHeader);

        if (_options.UpdateTraceIdentifier)
            context.TraceIdentifier = correlationId;

        // apply the correlation ID to the response header for client side tracking
        if (_options.IncludeInResponse)
            context.Response.OnStarting(() =>
            {
                if (context.Response.Headers.ContainsKey(_options.ResponseHeader))
                    return Task.CompletedTask;

                context.Response.Headers.Add(_options.ResponseHeader, correlationId);
                return Task.CompletedTask;
            });

        using (factory)
        {
            if (!_options.AddToLoggingScope || string.IsNullOrEmpty(_options.LoggingScopeKey))
            {
                await _next(context);
                return;
            }

            var state = new Dictionary<string, object> { [_options.LoggingScopeKey] = correlationId };

            using (_logger.BeginScope(state))
                await _next(context);
        }
    }

    private bool TryGetIdFromHeader(HttpContext context, out StringValues header) =>
        context.Request.Headers.TryGetValue(_options.RequestHeader, out header) && !StringValues.IsNullOrEmpty(header);

    private string GetOrCreateId(StringValues header) =>
        _options.IgnoreRequestHeader
            ? _provider.GenerateCorrelationId()
            : header.FirstOrDefault() ?? _provider.GenerateCorrelationId();

    private async Task WriteHeaderNotFoundError(HttpContext context)
    {
        if (_logger.IsEnabled(LogLevel.Error))
            _logger.LogError("The '{Header}' request header is required, but was not found.", _options.RequestHeader);

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response
            .WriteAsync($"The '{_options.RequestHeader}' request header is required, but was not found.");
    }
}
