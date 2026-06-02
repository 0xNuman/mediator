using FluentValidation;
using Mediator.Abstractions;
using Mediator.Extensions.FluentValidation;
using NSubstitute;

namespace Mediator.Extensions.FluentValidation.Tests;

public class ValidationBehaviourTests
{
    public class TestRequest : IRequest<string>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsRegistered()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var sut = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = "Valid" };
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("Success");
        };

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Success", result);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var sut = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = "" }; // Invalid
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => sut.Handle(request, next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var sut = new ValidationBehaviour<TestRequest, string>(validators);
        var request = new TestRequest { Name = "Valid" };
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("Success");
        };

        // Act
        var result = await sut.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal("Success", result);
    }
}
