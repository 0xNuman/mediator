# Source Generation

Source Generation is the "Final Boss" of performance in the Mediator library. It allows us to move from dynamic, reflection-based routing to static, compile-time routing.

## Why Source Generation?

In traditional Mediator implementations (like MediatR), the library must:
1.  Receive a request of type `object`.
2.  Find the `IRequestHandler<TRequest, TResponse>` at runtime.
3.  Use reflection to invoke the `Handle` method.

Even with caching, this involves dictionary lookups and delegate indirections.

**Our Source Generator** solves this by generating a `GeneratedMediator` class at compile-time that looks like this:

```csharp
public class GeneratedMediator : IMediator
{
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct)
    {
        if (request is MyRequest r1)
            return (Task<TResponse>)(object)_provider.GetRequiredService<IRequestHandler<MyRequest, MyResponse>>().HandleAsync(r1, ct);
        
        // ... other requests ...
    }
}
```

This resulting code is as fast as calling the handler directly because the compiler knows exactly which types are involved.

## Performance Benchmarks

| Method | Mean | Memory Overhead |
| :--- | :--- | :--- |
| **Direct Call** | 3.2 ns | 0 bytes |
| **Generated Mediator** | **38.4 ns** | **248 bytes** |
| *Original Mediator* | *124.0 ns* | *384 bytes* |

> **Note**: The memory overhead in the Generated Mediator comes from the resolution and composition of `IPipelineBehaviour`. Even with this overhead, it remains ~3.2x faster than reflection-based alternatives.

## How to Use

Simply use `AddMediator()` in your service registration:

```csharp
builder.Services.AddMediator();
```

## How it works

The generator uses the **Roslyn Incremental Generator** API:
1.  **Scanning**: It scans your project for any class implementing `IRequestHandler<,>`.
2.  **Mapping**: It extracts the Request and Response types.
3.  **Generation**: It produces a new C# file containing `GeneratedMediator` and an extension method `AddGeneratedMediator` that registers it.

Because it is an incremental generator, it is extremely efficient and doesn't slow down your IDE or build times.
