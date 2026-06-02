using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class PipelineInterruptionTests
{
    public class TestRequest : IRequest<string> { }

    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult("HandlerExecuted");
        }
    }

    public class InterruptionBehaviour : IPipelineBehaviour<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, RequestHandlerDelegate<string> next, CancellationToken cancellationToken)
        {
            return Task.FromResult("Interrupted");
        }
    }

    [Fact]
    public async Task Send_ShouldInterruptPipeline_WhenBehaviorDoesNotCallNext()
    {
        // Arrange
        var builder = new ServiceCollection().AddMediator(); var services = builder.Services;
        services.AddScoped<IPipelineBehaviour<TestRequest, string>, InterruptionBehaviour>();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var request = new TestRequest();

        // Act
        var result = await mediator.SendAsync(request);

        // Assert
        Assert.Equal("Interrupted", result);
    }
}
