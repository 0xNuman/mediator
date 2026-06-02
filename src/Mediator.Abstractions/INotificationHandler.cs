namespace Mediator.Abstractions;

/// <summary>
/// Represents a handler for processing notifications.
/// </summary>
/// <typeparam name="TNotification"></typeparam>
public interface INotificationHandler<in TNotification> where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}
