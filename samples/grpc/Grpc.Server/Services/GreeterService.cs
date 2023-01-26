using System.Text;
using Grpc.Core;

namespace Grpc.Server.Services;

public sealed class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger) =>
        _logger = logger;

    public override Task<HelloReply> Unary(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Unary {$request}", request);

        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}"
        });
    }

    public override async Task<HelloReply> ClientStreaming(IAsyncStreamReader<HelloRequest> requestStream,
        ServerCallContext context)
    {
        var sb = new StringBuilder();
        while (await requestStream.MoveNext())
        {
            var request = requestStream.Current;
            _logger.LogInformation("ClientStreaming {$request}", request);
            sb.AppendLine(request.Name);
        }

        return new HelloReply
        {
            Message = sb.ToString()
        };
    }

    public override async Task ServerStreaming(HelloRequest request, IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        foreach (var i in Enumerable.Range(1, 5))
        {
            _logger.LogInformation("ServerStreaming {i} {$request}", i, request);

            var message = new HelloReply { Message = $"Hello {request.Name} {i}" };
            await responseStream.WriteAsync(message);
        }
    }

    public override async Task DuplexStreaming(IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
    {
        var i = 1;
        while (await requestStream.MoveNext())
        {
            var request = requestStream.Current;
            _logger.LogInformation("DuplexStreaming request {i} {$request}", i, request);

            await responseStream
                .WriteAsync(new HelloReply { Message = $"DuplexStreaming response {request.Name} {i}" });

            i++;
        }
    }
}
