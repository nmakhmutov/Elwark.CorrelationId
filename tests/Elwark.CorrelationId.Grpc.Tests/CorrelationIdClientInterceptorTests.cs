using System.Text;
using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Grpc.Interceptors;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace Elwark.CorrelationId.Grpc.Tests;

public sealed class CorrelationIdClientInterceptorTests
{
    private static readonly Marshaller<string> StringMarshaller = Marshallers.Create(
        value => Encoding.UTF8.GetBytes(value),
        payload => Encoding.UTF8.GetString(payload)
    );

    [Fact]
    public void BlockingUnaryCall_DoesNotAddHeader_WhenCorrelationContextMissing()
    {
        var accessor = new TestCorrelationContextAccessor();
        var interceptor = CreateInterceptor(accessor);
        var context = CreateClientContext(MethodType.Unary, headers: null);

        Metadata? capturedHeaders = null;

        _ = interceptor.BlockingUnaryCall(
            "request",
            context,
            (_, callContext) =>
            {
                capturedHeaders = callContext.Options.Headers;
                return "response";
            });

        Assert.Null(capturedHeaders);
    }

    [Fact]
    public void BlockingUnaryCall_AddsRequestHeader_WhenCorrelationContextExists()
    {
        var accessor = new TestCorrelationContextAccessor
        {
            CorrelationContext = new CorrelationContext("cid-1", "X-Correlation-Id")
        };
        var interceptor = CreateInterceptor(
            accessor,
            requestHeader: "x-request-id",
            responseHeader: "x-response-id"
        );
        var context = CreateClientContext(MethodType.Unary, headers: null);

        Metadata? capturedHeaders = null;

        _ = interceptor.BlockingUnaryCall(
            "request",
            context,
            (_, callContext) =>
            {
                capturedHeaders = callContext.Options.Headers;
                return "response";
            });

        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders!, entry => entry is { Key: "x-response-id", Value: "cid-1" });
        Assert.DoesNotContain(capturedHeaders!, entry => entry.Key == "x-request-id");
    }

    [Fact]
    public void BlockingUnaryCall_DoesNotDuplicateHeader_WhenHeaderAlreadyExists()
    {
        var accessor = new TestCorrelationContextAccessor
        {
            CorrelationContext = new CorrelationContext("cid-1", "x-request-id")
        };
        var interceptor = CreateInterceptor(accessor, requestHeader: "x-request-id");
        var existingHeaders = new Metadata
        {
            new("x-request-id", "existing")
        };
        var context = CreateClientContext(MethodType.Unary, existingHeaders);

        Metadata? capturedHeaders = null;

        _ = interceptor.BlockingUnaryCall(
            "request",
            context,
            (_, callContext) =>
            {
                capturedHeaders = callContext.Options.Headers;
                return "response";
            });

        Assert.NotNull(capturedHeaders);
        var matches = capturedHeaders!
            .Where(entry => entry.Key == "x-request-id")
            .ToArray();

        Assert.Single(matches);
        Assert.Equal("existing", matches[0].Value);
    }

    [Fact]
    public void AsyncDuplexStreamingCall_AddsHeader_WhenCorrelationContextExists()
    {
        var accessor = new TestCorrelationContextAccessor
        {
            CorrelationContext = new CorrelationContext("cid-1", "x-request-id")
        };
        var interceptor = CreateInterceptor(accessor, requestHeader: "x-request-id");
        var context = CreateClientContext(MethodType.DuplexStreaming, headers: null);

        Metadata? capturedHeaders = null;

        _ = interceptor.AsyncDuplexStreamingCall(
            context,
            callContext =>
            {
                capturedHeaders = callContext.Options.Headers;
                return new AsyncDuplexStreamingCall<string, string>(
                    new NullClientStreamWriter<string>(),
                    new EmptyAsyncStreamReader<string>(),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                );
            });

        Assert.NotNull(capturedHeaders);
        Assert.Contains(capturedHeaders!, entry => entry is { Key: "x-response-id", Value: "cid-1" });
    }

    private static CorrelationIdClientInterceptor CreateInterceptor(
        ICorrelationContextAccessor accessor,
        string requestHeader = "x-request-id",
        string responseHeader = "x-response-id") =>
        new(
            accessor,
            Options.Create(
                new CorrelationIdOptions
                {
                    RequestHeader = requestHeader,
                    ResponseHeader = responseHeader
                }
            )
        );

    private static ClientInterceptorContext<string, string> CreateClientContext(MethodType methodType, Metadata? headers)
    {
        var method = new Method<string, string>(
            methodType,
            "test.Service",
            "testMethod",
            StringMarshaller,
            StringMarshaller
        );

        return new ClientInterceptorContext<string, string>(
            method,
            "localhost",
            new CallOptions(headers)
        );
    }

    private sealed class TestCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public CorrelationContext? CorrelationContext { get; set; }
    }

    private sealed class EmptyAsyncStreamReader<T> : IAsyncStreamReader<T>
    {
        public T Current => default!;

        public Task<bool> MoveNext(CancellationToken cancellationToken) =>
            Task.FromResult(false);
    }

    private sealed class NullClientStreamWriter<T> : IClientStreamWriter<T>
    {
        public WriteOptions? WriteOptions { get; set; }

        public Task WriteAsync(T message) => Task.CompletedTask;

        public Task CompleteAsync() => Task.CompletedTask;
    }
}
