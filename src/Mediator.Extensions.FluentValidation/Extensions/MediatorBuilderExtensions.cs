using Mediator.Abstractions;
using Mediator.Extensions.FluentValidation;
using FluentValidation;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring FluentValidation with Mediator.
/// </summary>
public static class MediatorBuilderExtensions
{
    /// <summary>
    /// Adds FluentValidation support to the Mediator pipeline.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IMediatorBuilder AddFluentValidation(this IMediatorBuilder builder) =>
        AddFluentValidation(builder, Assembly.GetCallingAssembly());

    /// <summary>
    /// Adds FluentValidation support to the Mediator pipeline.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IMediatorBuilder AddFluentValidation(this IMediatorBuilder builder, Assembly assembly)
    {
        // Register the pipeline behaviour.
        builder.Services.AddScoped(typeof(IPipelineBehaviour<,>), typeof(ValidationBehaviour<,>));

        // Register the validators.
        builder.Services.AddValidatorsFromAssembly(assembly);

        return builder;
    }
}
