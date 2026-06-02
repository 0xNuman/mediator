using FluentValidation;
using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Tests;

public class FullStackIntegrationTests
{
    public class IntegrationRequest : IRequest<string>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class IntegrationRequestValidator : AbstractValidator<IntegrationRequest>
    {
        public IntegrationRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        }
    }

    public class IntegrationRequestHandler : IRequestHandler<IntegrationRequest, string>
    {
        public Task<string> HandleAsync(IntegrationRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult($"Hello, {request.Name}");
        }
    }

    [Fact]
    public async Task FullStack_ShouldScanRegisterValidateAndExecute()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // 1. Scan and add Mediator
        services.AddMediator(typeof(FullStackIntegrationTests).Assembly);
        
        // 2. Add FluentValidation with assembly scanning
        services.AddFluentValidation(typeof(FullStackIntegrationTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        // Act & Assert - Case 1: Validation Failure
        var invalidRequest = new IntegrationRequest { Name = "Hi" }; // Too short
        await Assert.ThrowsAsync<ValidationException>(() => mediator.SendAsync(invalidRequest));

        // Act & Assert - Case 2: Success
        var validRequest = new IntegrationRequest { Name = "Mediator" };
        var result = await mediator.SendAsync(validRequest);
        
        Assert.Equal("Hello, Mediator", result);
    }
}
