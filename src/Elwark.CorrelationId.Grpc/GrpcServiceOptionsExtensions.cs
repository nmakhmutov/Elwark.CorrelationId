using Elwark.CorrelationId.Grpc.Interceptors;

// ReSharper disable CheckNamespace

namespace Grpc.AspNetCore.Server;

public static class GrpcServiceOptionsExtensions
{
    public static GrpcServiceOptions UseCorrelationId(this GrpcServiceOptions options)
    {
        options.Interceptors.Add<CorrelationIdServerInterceptor>();
        return options;
    }
}
