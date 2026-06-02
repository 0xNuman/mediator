using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Mediator.Tests;

public class CancellationTests
{
    public class TestRequest : IRequest<string> { }

    [Fact]
    public async Task Send_ShouldPropagateCancellationToken_ToHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = Substitute.For<IRequestHandler<TestRequest, string>>();
        services.AddSingleton(handler);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel

        // Act
        await mediator.SendAsync(request, cts.Token);

        // Assert
        await handler.Received(1).HandleAsync(Arg.Any<TestRequest>(), cts.Token);
    }

    [Fact]
    public async Task Send_ShouldPropagateCancellationToken_ToBehaviors()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = Substitute.For<IRequestHandler<TestRequest, string>>();
        services.AddSingleton(handler);

        var behavior = Substitute.For<IPipelineBehaviour<TestRequest, string>>();
        behavior.Handle(Arg.Any<TestRequest>(), Arg.Any<RequestHandlerDelegate<string>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => ((RequestHandlerDelegate<string>)callInfo[1])());

        services.AddSingleton(behavior);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();
        using var cts = new CancellationTokenSource();

        // Act
        await mediator.SendAsync(request, cts.Token);

        // Assert
        await behavior.Received(1).Handle(Arg.Any<TestRequest>(), Arg.Any<RequestHandlerDelegate<string>>(), cts.Token);
    }
}
