using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.Http.Providers;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static ICorrelationIdBuilder WithTraceIdentifierProvider(this ICorrelationIdBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<ICorrelationIdProvider, TraceIdCorrelationIdProvider>();

        return builder;
    }
}
