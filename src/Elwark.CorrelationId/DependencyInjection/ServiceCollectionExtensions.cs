using Elwark.CorrelationId;
using Elwark.CorrelationId.Abstractions;
using Elwark.CorrelationId.DependencyInjection;
using Elwark.CorrelationId.Providers;

// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     This operation is idempotent - multiple invocations will still only result in a single
    ///     instance of the required services in the <see cref="IServiceCollection" />. It can be invoked
    ///     multiple times in order to get access to the <see cref="ICorrelationIdBuilder" /> in multiple places.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the correlation ID services to.</param>
    /// <returns>
    ///     An instance of <see cref="ICorrelationIdBuilder" /> which to be used to configure correlation ID providers and
    ///     options.
    /// </returns>
    public static ICorrelationIdBuilder AddCorrelationId(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddSingleton<ICorrelationIdProvider, GuidCorrelationIdProvider>();
        services.AddTransient<ICorrelationContextFactory, CorrelationContextFactory>();

        return new CorrelationIdBuilder(services);
    }

    /// <summary>
    ///     Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     This operation is idempotent - multiple invocations will still only result in a single
    ///     instance of the required services in the <see cref="IServiceCollection" />. It can be invoked
    ///     multiple times in order to get access to the <see cref="ICorrelationIdBuilder" /> in multiple places.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the correlation ID services to.</param>
    /// <param name="configure">
    ///     The <see cref="Action{CorrelationIdOptions}" /> to configure the provided
    ///     <see cref="CorrelationIdOptions" />.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="ICorrelationIdBuilder" /> which to be used to configure correlation ID providers and
    ///     options.
    /// </returns>
    public static ICorrelationIdBuilder AddCorrelationId(this IServiceCollection services,
        Action<CorrelationIdOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        services.Configure(configure);

        return services.AddCorrelationId();
    }

    /// <summary>
    ///     Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection" />.
    /// </summary>
    /// <remarks>
    ///     This operation is idempotent - multiple invocations will still only result in a single
    ///     instance of the required services in the <see cref="IServiceCollection" />. It can be invoked
    ///     multiple times in order to get access to the <see cref="ICorrelationIdBuilder" /> in multiple places.
    /// </remarks>
    /// <typeparam name="T">The <see cref="ICorrelationIdProvider" /> implementation type.</typeparam>
    /// <param name="builder">The <see cref="ICorrelationIdBuilder" /> to add the correlation ID services to.</param>
    /// <returns>
    ///     An instance of <see cref="ICorrelationIdBuilder" /> which to be used to configure correlation ID providers and
    ///     options.
    /// </returns>
    public static ICorrelationIdBuilder WithCustomProvider<T>(this ICorrelationIdBuilder builder)
        where T : class, ICorrelationIdProvider
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        builder.Services.AddSingleton<ICorrelationIdProvider, T>();

        return builder;
    }
}
