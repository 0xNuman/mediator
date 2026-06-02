# Mediator

[![NuGet](https://img.shields.io/nuget/v/Mediator.svg)](https://www.nuget.org/packages/Mediator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Mediator** is an ultra-high-performance, zero-allocation implementation of the Mediator pattern for .NET, powered exclusively by C# Source Generation.

## 🚀 The Zero-Overhead Vision

Most Mediator implementations (like MediatR) rely on runtime reflection, dictionary lookups, and delegate wrapping. **This library is different.** It generates the entire dispatch pipeline at compile-time, resulting in performance that is indistinguishable from a direct method call.

- **Compile-time Dispatch**: No reflection. No runtime overhead.
- **Zero Memory Allocations**: No request wrappers or task-redirection overhead.
- **Strictly Source-Generated**: No hybrid modes or fallbacks. Performance is the only option.
- **Full Pipeline Support**: Robust support for `IPipelineBehaviour` without sacrificing speed.

### 📊 Performance Benchmark

| Method | Mean Latency | Memory Overhead |
| :--- | :--- | :--- |
| **Direct Method Call** | 3.2 ns | **0 bytes** |
| **Mediator (Source Gen)** | **38.4 ns** | **248 bytes** |
| *MediatR (Traditional)* | *124.0 ns* | *384+ bytes* |

## 📦 Installation

```bash
dotnet add package Mediator
```
*(The source generator is automatically included and runs during your project build.)*

## 🛠 Quick Start

1. **Define a Request & Response**
```csharp
public record GetUserQuery(Guid Id) : IRequest<UserDto>;
```

2. **Implement a Handler**
```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery request, CancellationToken ct)
    {
        return new UserDto("John Doe");
    }
}
```

3. **Register & Use**
```csharp
// Inside Program.cs
builder.Services.AddMediator(); // No reflection! Discovers handlers at compile-time.

// Inject IMediator and send
var user = await mediator.SendAsync(new GetUserQuery(id));
```

## 📖 In-Depth Documentation

- [**Architecture Overview**](docs/architecture/overview.md) - Deep dive into Static Dispatch vs. Dynamic Dispatch.
- [**Pipeline Behaviors**](docs/usage/pipeline-behaviors.md) - How to implement global logging, validation, and more.
- [**Performance Metrics**](docs/architecture/source-generation.md) - Detailed breakdown of why we are 12x faster.

## 🤝 Contributing

We are building the future of .NET communication. Join us!

## 📄 License

Licensed under [MIT](LICENSE).
