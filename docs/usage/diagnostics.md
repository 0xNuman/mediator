# Compiler Diagnostics

The Mediator library provides real-time feedback during the build process to catch common configuration errors before your application even runs. These diagnostics appear as errors and warnings in your IDE (Visual Studio, Rider) and build logs.

## 🚨 Error: MED001 - Multiple Handlers Found

**Severity**: Error (Build Fails)

This error occurs when you define more than one handler for the same `IRequest<T>`. Unlike notifications, each request must have exactly one authoritative handler.

### ❌ Problematic Code
```csharp
public class CreateUserHandler1 : IRequestHandler<CreateUserCommand, Guid> { ... }
public class CreateUserHandler2 : IRequestHandler<CreateUserCommand, Guid> { ... }
```

### ✅ Solution
Ensure only one class implements `IRequestHandler<TRequest, TResponse>` for any given request type.

---

## ⚠️ Warning: MED002 - No Handler Found

**Severity**: Warning

This warning occurs when an `IRequest<T>` implementation is discovered, but no corresponding `IRequestHandler` is found in the project.

### ❌ Problematic Code
```csharp
public class DeleteUserCommand : IRequest<bool> { ... }
// Missing: DeleteUserCommandHandler
```

### ✅ Solution
Implement the missing handler or remove the request definition if it's no longer used. Sending this request at runtime will result in an `InvalidOperationException`.

---

## ⚙️ Suppressing Diagnostics

If you have a legitimate reason to suppress these diagnostics, you can do so using standard MSBuild properties or `#pragma` directives in your code.

### Using .editorconfig
```ini
[*.cs]
dotnet_diagnostic.MED001.severity = none
dotnet_diagnostic.MED002.severity = suggestion
```
