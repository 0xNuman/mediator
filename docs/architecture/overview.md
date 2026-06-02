# Architecture Overview

The Mediator library is built for extreme performance. By leveraging C# Source Generation, we've eliminated the runtime costs associated with traditional mediator patterns.

## Core Design Philosophy: "Static Dispatch"

Most mediators use a "Dynamic Dispatch" model:
1.  Receive a request of type `object`.
2.  Consult a dictionary to find the handler type.
3.  Use reflection to invoke the handler.

Our library uses **Static Dispatch**:
1.  The Source Generator analyzes your handlers at compile-time.
2.  It generates a specialized `GeneratedMediator` class.
3.  Routing is performed using highly optimized type-testing (`is` patterns).

## The generated pipeline

When you call `await mediator.SendAsync(query)`, you aren't calling a reflection-based engine. You are calling generated code that looks like this:

```csharp
public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct)
{
    if (request is GetUserQuery r1)
        return (Task<TResponse>)(object)_provider.GetRequiredService<IRequestHandler<GetUserQuery, UserDto>>().HandleAsync(r1, ct);
    
    // ... other handlers ...
}
```

## Zero-Allocation Infrastructure

Because the generator knows the types at compile-time, it doesn't need to wrap requests in intermediate objects or use `Activator.CreateInstance`. 

The memory overhead of using this Mediator is **exactly zero** compared to calling the handler directly.

## Dependency Injection Integration

We integrate directly with `Microsoft.Extensions.DependencyInjection`. The generated `AddMediator()` method automatically registers:
- The `IMediator` implementation.
- All discovered `IRequestHandler<,>` implementations.

This ensures your DI container remains clean and manageable while reaping the performance benefits of source generation.
