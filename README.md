# Mediator

[![NuGet](https://img.shields.io/nuget/v/Mediator.svg)](https://www.nuget.org/packages/Mediator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Mediator** is an ultra-high-performance, zero-allocation implementation of the Mediator pattern for .NET, powered exclusively by C# Source Generation.

## 🚀 The Zero-Overhead Vision

Most Mediator implementations (like MediatR) rely on runtime reflection, dictionary lookups, and delegate wrapping. **This library is different.** It generates the entire dispatch pipeline at compile-time, resulting in performance that is indistinguishable from a direct method call.

- **Compile-time Dispatch**: No reflection. No runtime overhead.
- **Zero Memory Allocations**: No request wrappers or task-redirection overhead.
- **Strictly Source-Generated**: No hybrid modes or fallbacks. Performance is the only option.
- **Pro Compiler Diagnostics**: Catch configuration errors (ambiguous/missing handlers) at compile-time.
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
    public async Task<UserDto> HandleAsync(GetUserQuery request, CancellationToken ct) => new UserDto("John Doe");
}
```

3. **(Optional) Define a Notification & Multiple Handlers**
```csharp
public record UserCreated(Guid Id) : INotification;
public class EmailHandler : INotificationHandler<UserCreated> { /* ... */ }
public class AuditHandler : INotificationHandler<UserCreated> { /* ... */ }
```

4. **Register & Use**
```csharp
builder.Services.AddMediator();

// Send a request
var user = await mediator.SendAsync(new GetUserQuery(id));

// Publish a notification
await mediator.PublishAsync(new UserCreated(user.Id));
```

## 📖 Detailed Documentation

- [**Architecture Overview**](docs/architecture/overview.md) - Static Dispatch vs. Dynamic Dispatch.
- [**Getting Started**](docs/usage/getting-started.md) - Full setup guide.
- [**Notifications**](docs/usage/notifications.md) - Implementing event-driven systems (Pub/Sub).
- [**Pipeline Behaviors**](docs/usage/pipeline-behaviors.md) - Cross-cutting concerns (logging, validation).
- [**Compiler Diagnostics**](docs/usage/diagnostics.md) - Real-time build-time error catching.
- [**Configuration**](docs/usage/configuration.md) - Customizing namespaces.
- [**Performance Metrics**](docs/architecture/source-generation.md) - Detailed breakdown of why we are 12x faster.

## 🤝 Contributing

We are building the future of .NET communication. Join us!

## 📄 License

Licensed under [MIT](LICENSE).
