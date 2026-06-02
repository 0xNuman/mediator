# Notifications (Pub/Sub)

Notifications allow you to implement event-driven architectures by broadcasting messages to multiple handlers. This is a "one-to-many" communication pattern.

## 🛠 Usage

### 1. Define a Notification
Implement the `INotification` interface. Use `record` for clean, immutable event definitions.

```csharp
public record UserRegisteredEvent(Guid UserId, string Email) : INotification;
```

### 2. Implement Multiple Handlers
Implement `INotificationHandler<T>` for each action that should occur when the event is published.

```csharp
public class WelcomeEmailHandler : INotificationHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent notification, CancellationToken ct)
    {
        // Send welcome email...
    }
}

public class UserSearchIndexHandler : INotificationHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent notification, CancellationToken ct)
    {
        // Update search index...
    }
}
```

### 3. Publish the Notification
Inject `IMediator` and call `PublishAsync`.

```csharp
public async Task RegisterUser(UserDto user, IMediator mediator)
{
    // ... logic to save user ...
    
    await mediator.PublishAsync(new UserRegisteredEvent(user.Id, user.Email));
}
```

## ⚙️ How it Works

The Source Generator automatically discovers all classes implementing `INotificationHandler<T>`. It then generates a specialized `PublishAsync` method in the `GeneratedMediator` that:
1.  **Identifies** the notification type at runtime using fast type-testing.
2.  **Resolves** all registered handlers from the DI container.
3.  **Executes** each handler sequentially.

## ⚡ Performance Note

Just like Request/Response, the Notification engine is strictly source-generated. This means there is **zero reflection** used during the dispatch process, ensuring that event broadcasting remains ultra-fast even as your system grows to support hundreds of events and handlers.
