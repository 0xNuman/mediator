# Mediator Project Context

This file provides foundational mandates and architectural context for the **Mediator** project. It is intended to guide all future AI-driven interactions within this repository.

## 🎯 Project Overview

**Mediator** is a high-performance, zero-allocation implementation of the Mediator pattern for .NET, powered exclusively by **C# Source Generation**. 

### Core Philosophy: Static Dispatch
Unlike traditional libraries (e.g., MediatR) that use runtime reflection and dictionary lookups to route messages, this library uses **Static Dispatch**. The entire routing pipeline is generated at compile-time, resulting in performance that matches direct method calls and incurs **zero extra memory allocations**.

### Key Technologies
- **Runtime**: .NET 10.0
- **Source Generation**: Roslyn Incremental Generators (netstandard2.0)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Validation**: FluentValidation

---

## 🏗 Architecture & Project Structure

The solution is divided into four main projects:

1.  **`src/Mediator.Abstractions`**: Defines the core contracts (`IMediator`, `IRequest`, `INotification`, etc.).
2.  **`src/Mediator.SourceGenerator`**: The Roslyn-powered engine that analyzes user code and generates the `GeneratedMediator` dispatcher and registration logic.
3.  **`src/Mediator`**: The primary entry point for consumers. It bundles the abstractions and the source generator.
4.  **`src/Mediator.Extensions.FluentValidation`**: Provides built-in support for request validation via pipeline behaviors.

### The Implicit Flow
The library uses `Directory.Build.targets` to automatically inject the Source Generator into any project that references the `Mediator` project. This ensures a seamless "zero-config" developer experience.

---

## 🛠 Building & Testing

### Key Commands
- **Build Solution**: `dotnet build`
- **Run Unit Tests**: `dotnet test`
- **Run Benchmarks**: `dotnet run -c Release --project test/Mediator.Benchmarks/Mediator.Benchmarks.csproj`
- **Clean Artifacts**: `rm -rf src/*/obj src/*/bin test/*/obj test/*/bin`

### Testing Conventions
- All handlers must be concrete classes for the Source Generator to discover them in tests.
- Mocking handlers requires a proxy implementation or using `IMediator` from a built ServiceProvider to verify the generated dispatch logic.

---

## 📏 Development Conventions

### 1. No Reflection
NEVER introduce runtime reflection for request/notification dispatching. All routing MUST be handled by the Source Generator.

### 2. Source Generation Best Practices
- **Isolation**: Generated code resides in the `Mediator.Generated` namespace by default (customizable via `MediatorOptionsAttribute`).
- **Robustness**: Always use `global::` qualified names (e.g., `global::System.Threading.Tasks.Task`) in generated code to avoid namespace collisions.
- **Incrementalism**: Ensure the `MediatorGenerator` remains an `IIncrementalGenerator` for optimal IDE performance.

### 3. Documentation First
- The root `README.md` is the high-level entry point.
- Detailed technical documentation is maintained in the `docs/` folder, divided into `architecture/` and `usage/`.
- Every new feature MUST be documented in both the `README.md` and the relevant `docs/` file.

### 4. Performance as a Hard Constraint
The library's identity is built on performance. Any change to the core dispatch path MUST be verified against the `Mediator.Benchmarks` to ensure no regressions in latency or memory allocations.
