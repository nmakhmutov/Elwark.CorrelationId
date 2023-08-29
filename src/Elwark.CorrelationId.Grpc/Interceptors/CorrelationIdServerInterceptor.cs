using System.Diagnostics.CodeAnalysis;
using Elwark.CorrelationId.Abstractions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elwark.CorrelationId.Grpc.Interceptors;

internal sealed class CorrelationIdServerInterceptor : Interceptor
{
    private readonly ICorrelationContextFactory _factory;
    private readonly ILogger<CorrelationIdServerInterceptor> _logger;
    private readonly CorrelationIdOptions _options;
    private readonly ICorrelationIdProvider _provider;

    public CorrelationIdServerInterceptor(ICorrelationIdProvider provider,
        ILogger<CorrelationIdServerInterceptor> logger,
        IOptions<CorrelationIdOptions> options, ICorrelationContextFactory factory)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(context, () => base.UnaryServerHandler(request, context, continuation));

    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(context, () => base.ClientStreamingServerHandler(requestStream, context, continuation));

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(context, () => base.ServerStreamingServerHandler(request, responseStream, context, continuation));

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation) =>
        HandleAsync(context,
            () => base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation));

    private async Task<T> HandleAsync<T>(ServerCallContext context, Func<Task<T>> response)
    {
        var hasCorrelationId = TryGetIdFromMetadata(context.RequestHeaders, out var header);
        if (_options.EnforceHeader && !hasCorrelationId)
            ThrowHeaderNotFoundException();

        using (_factory)
        {
            var correlation = _factory.Create(GetOrCreateId(header), _options.RequestHeader);

            if (_options.IncludeInResponse && !TryGetIdFromMetadata(context.ResponseTrailers, out _))
                await context.WriteResponseHeadersAsync(new Metadata
                {
                    new(_options.ResponseHeader, correlation.CorrelationId)
                });

            if (!_options.AddToLoggingScope)
                return await response();

            var state = new Dictionary<string, object> { [_options.LoggingScopeKey] = correlation.CorrelationId };
            using (_logger.BeginScope(state))
                return await response();
        }
    }

    private async Task HandleAsync(ServerCallContext context, Func<Task> response)
    {
        var hasCorrelationId = TryGetIdFromMetadata(context.RequestHeaders, out var header);
        if (_options.EnforceHeader && !hasCorrelationId)
            ThrowHeaderNotFoundException();

        using (_factory)
        {
            var correlation = _factory.Create(GetOrCreateId(header), _options.RequestHeader);

            if (_options.IncludeInResponse && !TryGetIdFromMetadata(context.ResponseTrailers, out _))
                await context.WriteResponseHeadersAsync(new Metadata
                {
                    new(_options.ResponseHeader, correlation.CorrelationId)
                });

            if (!_options.AddToLoggingScope)
            {
                await response();
                return;
            }

            var state = new Dictionary<string, object> { [_options.LoggingScopeKey] = correlation.CorrelationId };
            using (_logger.BeginScope(state))
                await response();
        }
    }

    private bool TryGetIdFromMetadata(Metadata? metadata, [MaybeNullWhen(false)] out string header)
    {
        header = null;

        var cid = metadata?.FirstOrDefault(x =>
            string.Equals(x.Key, _options.RequestHeader, StringComparison.InvariantCultureIgnoreCase));

        if (string.IsNullOrEmpty(cid?.Value))
            return false;

        header = cid.Value;
        return true;
    }

    private string GetOrCreateId(string? header)
    {
        if (_options.IgnoreRequestHeader)
            return _provider.GenerateCorrelationId();

        return string.IsNullOrEmpty(header) ? _provider.GenerateCorrelationId() : header;
    }

    private void ThrowHeaderNotFoundException()
    {
        if (_logger.IsEnabled(LogLevel.Error))
            _logger.LogError("The '{Header}' request header is required, but was not found.", _options.RequestHeader);

        var message = $"The '{_options.RequestHeader}' request header is required, but was not found.";
        throw new RpcException(new Status(StatusCode.InvalidArgument, message));
    }
}
