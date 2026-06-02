using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Mediator.Tests;

public class PipelineInterruptionTests
{
    public class TestRequest : IRequest<string> { }

    [Fact]
    public async Task Send_ShouldInterruptPipeline_WhenBehaviorDoesNotCallNext()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = Substitute.For<IRequestHandler<TestRequest, string>>();
        services.AddSingleton(handler);

        // Behavior that interrupts (does not call next)
        var interruptingBehavior = Substitute.For<IPipelineBehaviour<TestRequest, string>>();
        interruptingBehavior.Handle(Arg.Any<TestRequest>(), Arg.Any<RequestHandlerDelegate<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("Interrupted"));

        services.AddSingleton(interruptingBehavior);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();

        // Act
        var result = await mediator.SendAsync(request);

        // Assert
        Assert.Equal("Interrupted", result);
        // Ensure handler was NOT called
        await handler.DidNotReceive().HandleAsync(Arg.Any<TestRequest>(), Arg.Any<CancellationToken>());
    }
}
