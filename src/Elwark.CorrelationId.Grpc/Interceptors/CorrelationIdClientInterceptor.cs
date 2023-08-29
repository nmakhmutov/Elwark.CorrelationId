using Elwark.CorrelationId.Abstractions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace Elwark.CorrelationId.Grpc.Interceptors;

internal sealed class CorrelationIdClientInterceptor : Interceptor
{
    private readonly ICorrelationContextAccessor _accessor;
    private readonly CorrelationIdOptions _options;

    public CorrelationIdClientInterceptor(ICorrelationContextAccessor accessor, IOptions<CorrelationIdOptions> options)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        if (_accessor.CorrelationContext is null || IsCorrelationIdExists(context.Options.Headers))
            return base.BlockingUnaryCall(request, context, continuation);

        var entry = new Metadata.Entry(_options.ResponseHeader, _accessor.CorrelationContext.CorrelationId);
        if (context.Options.Headers is null)
            context.Options.WithHeaders(new Metadata { entry });
        else
            context.Options.Headers.Add(entry);

        return base.BlockingUnaryCall(request, context, continuation);
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        if (_accessor.CorrelationContext is null || IsCorrelationIdExists(context.Options.Headers))
            return base.AsyncUnaryCall(request, context, continuation);

        var entry = new Metadata.Entry(_options.ResponseHeader, _accessor.CorrelationContext.CorrelationId);
        if (context.Options.Headers is null)
            return base.AsyncUnaryCall(
                request,
                new ClientInterceptorContext<TRequest, TResponse>(
                    context.Method,
                    context.Host,
                    context.Options.WithHeaders(new Metadata { entry })
                ),
                continuation
            );

        context.Options.Headers.Add(entry);
        return base.AsyncUnaryCall(request, context, continuation);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        if (_accessor.CorrelationContext is null || IsCorrelationIdExists(context.Options.Headers))
            return base.AsyncServerStreamingCall(request, context, continuation);

        var entry = new Metadata.Entry(_options.ResponseHeader, _accessor.CorrelationContext.CorrelationId);
        if (context.Options.Headers is null)
            return base.AsyncServerStreamingCall(
                request,
                new ClientInterceptorContext<TRequest, TResponse>(
                    context.Method,
                    context.Host,
                    context.Options.WithHeaders(new Metadata { entry })
                ),
                continuation
            );

        context.Options.Headers.Add(entry);
        return base.AsyncServerStreamingCall(request, context, continuation);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        if (_accessor.CorrelationContext is null || IsCorrelationIdExists(context.Options.Headers))
            return base.AsyncClientStreamingCall(context, continuation);

        var entry = new Metadata.Entry(_options.ResponseHeader, _accessor.CorrelationContext.CorrelationId);
        if (context.Options.Headers is null)
            return base.AsyncClientStreamingCall(
                new ClientInterceptorContext<TRequest, TResponse>(
                    context.Method,
                    context.Host,
                    context.Options.WithHeaders(new Metadata { entry })
                ),
                continuation
            );

        context.Options.Headers.Add(entry);
        return base.AsyncClientStreamingCall(context, continuation);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        if (_accessor.CorrelationContext is null || IsCorrelationIdExists(context.Options.Headers))
            return base.AsyncDuplexStreamingCall(context, continuation);

        var entry = new Metadata.Entry(_options.ResponseHeader, _accessor.CorrelationContext.CorrelationId);
        if (context.Options.Headers is null)
            return base.AsyncDuplexStreamingCall(
                new ClientInterceptorContext<TRequest, TResponse>(
                    context.Method,
                    context.Host,
                    context.Options.WithHeaders(new Metadata { entry })
                ),
                continuation
            );

        context.Options.Headers.Add(entry);
        return base.AsyncDuplexStreamingCall(context, continuation);
    }

    private bool IsCorrelationIdExists(Metadata? metadata)
    {
        if (metadata is null)
            return false;

        var cid = metadata.FirstOrDefault(x =>
            string.Equals(x.Key, _options.ResponseHeader, StringComparison.OrdinalIgnoreCase));

        return !string.IsNullOrEmpty(cid?.Value);
    }
}
