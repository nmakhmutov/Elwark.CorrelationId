using Microsoft.Extensions.DependencyInjection;

namespace Elwark.CorrelationId.Abstractions;

public interface ICorrelationIdBuilder
{
    IServiceCollection Services { get; }
}
