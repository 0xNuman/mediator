using Mediator.Abstractions;
using Mediator.Extensions.OpenTelemetry;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring OpenTelemetry with Mediator.
/// </summary>
public static class MediatorBuilderExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing to the Mediator pipeline.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IMediatorBuilder AddOpenTelemetry(this IMediatorBuilder builder)
    {
        builder.Services.AddScoped(typeof(IPipelineBehaviour<,>), typeof(OpenTelemetryBehaviour<,>));
        return builder;
    }
}
