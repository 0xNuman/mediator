namespace Mediator.Abstractions;

/// <summary>
/// Represents a mediator that facilitates communication between different components in a system.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to the mediator and returns a response.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
}