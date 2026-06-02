using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Abstractions;

/// <summary>
/// A builder for configuring Mediator services.
/// </summary>
public interface IMediatorBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }
}