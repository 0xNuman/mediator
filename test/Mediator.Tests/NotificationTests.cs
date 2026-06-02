using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class NotificationTests
{
    public class TestNotification : INotification
    {
        public List<string> HandlersExecuted { get; } = new();
    }

    public class NotificationHandler1 : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
        {
            notification.HandlersExecuted.Add("Handler1");
            return Task.CompletedTask;
        }
    }

    public class NotificationHandler2 : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
        {
            notification.HandlersExecuted.Add("Handler2");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Publish_ShouldExecuteAllRegisteredHandlers()
    {
        // Arrange
        var builder = new ServiceCollection().AddMediator(); var services = builder.Services;
        // The generator will find NotificationHandler1 and NotificationHandler2 
        // and register them automatically in AddMediator()
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var notification = new TestNotification();

        // Act
        await mediator.PublishAsync(notification);

        // Assert
        Assert.Contains("Handler1", notification.HandlersExecuted);
        Assert.Contains("Handler2", notification.HandlersExecuted);
        Assert.Equal(2, notification.HandlersExecuted.Count);
    }
}
