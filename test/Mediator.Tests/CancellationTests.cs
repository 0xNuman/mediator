using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Mediator.Tests;

public class CancellationTests
{
    public class TestRequest : IRequest<string> { }

    // Use a specific interface or implementation to avoid circular dependency
    public interface IProxyTarget : IRequestHandler<TestRequest, string> { }

    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        private readonly IProxyTarget _proxy;
        public TestRequestHandler(IProxyTarget proxy) => _proxy = proxy;
        public Task<string> HandleAsync(TestRequest r, CancellationToken ct) => _proxy.HandleAsync(r, ct);
    }

    [Fact]
    public async Task Send_ShouldPropagateCancellationToken_ToHandler()
    {
        // Arrange
        var services = new ServiceCollection().AddMediator();
        var mockHandler = Substitute.For<IProxyTarget>();
        services.AddSingleton(mockHandler);
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest();
        using var cts = new CancellationTokenSource();
        // Don't pre-cancel, cancel immediately after starting or just pass a token
        // to verify it reaches the mock.

        // Act
        await mediator.SendAsync(request, cts.Token);

        // Assert
        await mockHandler.Received(1).HandleAsync(Arg.Any<TestRequest>(), cts.Token);
    }

    [Fact]
    public async Task Send_ShouldPropagateCancellationToken_ToBehaviors()
    {
        // Arrange
        var services = new ServiceCollection().AddMediator();
        var mockHandler = Substitute.For<IProxyTarget>();
        services.AddSingleton(mockHandler);
        services.AddScoped<IRequestHandler<TestRequest, string>, TestRequestHandler>();

        var behavior = Substitute.For<IPipelineBehaviour<TestRequest, string>>();
        behavior.Handle(Arg.Any<TestRequest>(), Arg.Any<RequestHandlerDelegate<string>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => ((RequestHandlerDelegate<string>)callInfo[1])());

        services.AddSingleton(behavior);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest();
        using var cts = new CancellationTokenSource();

        // Act
        await mediator.SendAsync(request, cts.Token);

        // Assert
        await behavior.Received(1).Handle(Arg.Any<TestRequest>(), Arg.Any<RequestHandlerDelegate<string>>(), cts.Token);
    }
}
