using Elwark.CorrelationId.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Elwark.CorrelationId.DependencyInjection;

internal sealed class CorrelationIdBuilder : ICorrelationIdBuilder
{
    public CorrelationIdBuilder(IServiceCollection services) =>
        Services = services;

    public IServiceCollection Services { get; }
}
