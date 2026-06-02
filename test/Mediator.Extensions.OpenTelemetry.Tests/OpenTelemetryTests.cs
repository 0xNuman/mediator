using System.Diagnostics;
using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace Mediator.Extensions.OpenTelemetry.Tests;

public class OpenTelemetryTests
{
    public class TestRequest : IRequest<string> { }

    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult("Done");
        }
    }

    [Fact]
    public async Task SendAsync_ShouldCreateActivityWithTags()
    {
        // Arrange
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Mediator")
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var services = new ServiceCollection();
        services.AddMediator()
                .AddOpenTelemetry();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act
        await mediator.SendAsync(new TestRequest());

        // Assert
        tracerProvider.ForceFlush();
        var activity = Assert.Single(exportedActivities);
        Assert.Equal("Mediator TestRequest", activity.DisplayName);
        Assert.Equal(typeof(TestRequest).FullName, activity.GetTagItem("messaging.mediator.request_type"));
        Assert.Equal("mediator", activity.GetTagItem("messaging.system"));
    }

    [Fact]
    public async Task SendAsync_ShouldRecordExceptionInActivity()
    {
        // Arrange
        var exportedActivities = new List<Activity>();
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource("Mediator")
            .AddInMemoryExporter(exportedActivities)
            .Build();

        var services = new ServiceCollection();
        services.AddMediator()
                .AddOpenTelemetry();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => mediator.SendAsync(new RequestThatThrows()));

        tracerProvider.ForceFlush();
        var activity = Assert.Single(exportedActivities);
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        
        var exceptionEvent = Assert.Single(activity.Events, e => e.Name == "exception");
        Assert.Equal("System.Exception", exceptionEvent.Tags.First(t => t.Key == "exception.type").Value);
        Assert.Equal("Boom", exceptionEvent.Tags.First(t => t.Key == "exception.message").Value);
    }

    public class RequestThatThrows : IRequest<string> { }

    public class ThrowingHandler : IRequestHandler<RequestThatThrows, string>
    {
        public Task<string> HandleAsync(RequestThatThrows request, CancellationToken cancellationToken)
        {
            throw new Exception("Boom");
        }
    }
}
