# OpenTelemetry Tracing

The Mediator library provides first-class support for distributed tracing via the `Mediator.Extensions.OpenTelemetry` package. This allows you to monitor and profile your message pipeline in real-time.

## 🚀 Getting Started

### 1. Install the Extension
```bash
dotnet add package Mediator.Extensions.OpenTelemetry
```

### 2. Register the Behavior
Use the fluent builder to enable tracing:

```csharp
builder.Services.AddMediator()
    .AddOpenTelemetry();
```

### 3. Configure OpenTelemetry
In your application startup, ensure you are listening to the `"Mediator"` activity source:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => 
    {
        tracing.AddSource("Mediator")
               .AddAspNetCoreInstrumentation()
               .AddOtlpExporter();
    });
```

## 🔍 What is Recorded?

Each `SendAsync` or `PublishAsync` call creates a new **Span** (Activity) with the following details:

- **Operation Name**: `Mediator {RequestName}` (e.g., `Mediator CreateUserCommand`).
- **Tags**:
    - `messaging.mediator.request_type`: The fully qualified name of the request.
    - `messaging.system`: always set to `mediator`.
- **Status**: Set to `Error` if an exception occurs.
- **Events**: Exceptions are recorded as events with type, message, and stack trace.

## ⚡ Performance Impact

The OpenTelemetry extension is designed for high-performance environments:
- **Opt-in**: If you don't call `.AddOpenTelemetry()`, there is zero overhead.
- **Efficient**: Uses `ActivitySource` which has near-zero overhead when no listeners are active.
- **Zero-Allocation (Metadata)**: Leveraging the source-generated engine, we avoid runtime reflection for tag metadata.
