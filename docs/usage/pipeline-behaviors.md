# Pipeline Behaviors

Pipeline behaviors are a core feature of the Mediator library, allowing you to implement cross-cutting concerns with zero impact on the maintainability of your handlers.

## 🛠 Implementing a Behavior

Implement the `IPipelineBehaviour<TRequest, TResponse>` interface. Use the `next` delegate to continue the execution.

```csharp
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0) throw new ValidationException(failures);

        return await next(); // Proceed to next behavior or handler
    }
}
```

## ⚙️ Registration

Behaviors are registered via Dependency Injection. The Mediator automatically detects all registered behaviors for a given request/response pair.

```csharp
// Behaviors execute in the reverse order of registration (LIFO)
builder.Services.AddScoped(typeof(IPipelineBehaviour<,>), typeof(ValidationBehaviour<,>));
builder.Services.AddScoped(typeof(IPipelineBehaviour<,>), typeof(LoggingBehaviour<,>));
```

## ⚡ Performance Note

Unlike other implementations that use nested delegates and runtime closures, our source-generated dispatcher minimizes the stack depth and avoids unnecessary allocations, even when multiple behaviors are present.
