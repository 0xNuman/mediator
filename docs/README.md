# Mediator Documentation

Welcome to the official documentation for the Mediator library—a high-performance, lightweight implementation of the Mediator pattern for .NET.

## 🚀 Key Features

- **Decoupling**: Separate request senders from handlers completely.
- **Pipeline Behaviors**: Cross-cutting concerns (logging, validation, caching) using a powerful decorator-based pipeline.
- **High Performance**: Optimized with runtime caching and soon-to-be-powered by Source Generation.
- **Fluent Validation**: Built-in support for automatic request validation.
- **Enterprise Grade**: Designed for scalability and maintainability.

## 📖 Table of Contents

1. [Architecture Overview](architecture/overview.md)
   - Understand the core design, the Request-Response bridge, and the Pipeline mechanism.
2. [Getting Started](usage/getting-started.md)
   - How to install, register, and send your first request.
3. [Notifications](usage/notifications.md)
   - Broadcast messages to multiple handlers (Pub/Sub).
4. [Pipeline Behaviors](usage/pipeline-behaviors.md)
   - How to intercept requests and implement global logic.
5. [Configuration](usage/configuration.md)
   - How to customize the generated namespace with `MediatorOptions`.
6. [Source Generation](architecture/source-generation.md)
   - Deep dive into how we achieve near-zero overhead.

## 🛠 Project Structure

- `src/Mediator.Abstractions`: Core interfaces.
- `src/Mediator`: Main implementation.
- `src/Mediator.Extensions.FluentValidation`: Validation support.
- `src/Mediator.SourceGenerator`: Compile-time optimizations.
