using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddMediator_ShouldRegisterHandlersFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(DependencyInjectionTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var handler = serviceProvider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.NotNull(handler);
        Assert.IsType<TestRequestHandler>(handler);
        
        var mediator = serviceProvider.GetService<IMediator>();
        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediator_ShouldRegisterAllImplementedInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(typeof(DependencyInjectionTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // TestRequestHandler implements IRequestHandler<TestRequest, string>
        var handler = serviceProvider.GetService<IRequestHandler<TestRequest, string>>();
        Assert.NotNull(handler);
    }
}
