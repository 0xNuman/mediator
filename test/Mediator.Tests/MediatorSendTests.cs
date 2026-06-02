using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class MediatorSendTests
{
    public class UnhandledRequest : IRequest<string> { }

    [Fact]
    public async Task Send_ShouldResolveAndExecuteCorrectHandler()
    {
        // Arrange
        var builder = new ServiceCollection().AddMediator(); var services = builder.Services;
        var serviceProvider = services.BuildServiceProvider();
        var sut = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest { Message = "Hello, Mediator!" };

        // Act
        var result = await sut.SendAsync(request);

        // Assert
        Assert.Equal("Hello, Mediator!", result);
    }

    [Fact]
    public async Task Send_ShouldThrowException_WhenNoHandlerIsRegistered()
    {
        // Arrange
        var builder = new ServiceCollection().AddMediator(); var services = builder.Services;
        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetRequiredService<IMediator>();
        var request = new UnhandledRequest();

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.SendAsync(request));
    }
}
