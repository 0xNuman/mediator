using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class PipelineBehaviourTests
{
    [Fact]
    public async Task Send_ShouldExecuteBehaviors_InCorrectOrder()
    {
        // Arrange
        var executionTracker = new List<string>();

        var builder = new ServiceCollection().AddMediator(); var services = builder.Services;
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();

        // Register behaviors. The DI container will provide them in the order they are registered.
        // Our Mediator reverses this list, so they execute like a stack (LIFO).
        services.AddScoped<IPipelineBehaviour<TestRequest, string>>(sp =>
            new TrackingBehaviour<TestRequest, string>(executionTracker, "Behavior1"));
        services.AddScoped<IPipelineBehaviour<TestRequest, string>>(sp =>
            new TrackingBehaviour<TestRequest, string>(executionTracker, "Behavior2"));

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Pipeline Test" };

        // Act
        await mediator.SendAsync(request);

        // Assert
        // Expected order: Behavior2 starts, then Behavior1 starts, then the handler runs, then Behavior1 finishes, then Behavior2 finishes.
        // Our simple handler doesn't add to the tracker, so we just see the behaviors.
        var expectedOrder = new List<string>
        {
            "Before-Behavior2",
            "Before-Behavior1",
            "After-Behavior1",
            "After-Behavior2"
        };

        Assert.Equal(expectedOrder, executionTracker);
    }
}