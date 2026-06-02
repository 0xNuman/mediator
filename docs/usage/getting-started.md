# Getting Started

Follow these steps to integrate the Mediator into your .NET application.

## 1. Installation

Install the core package:
```bash
dotnet add package Mediator
```

(Optional) Install FluentValidation support:
```bash
dotnet add package Mediator.Extensions.FluentValidation
```

## 2. Define Request and Handler

Create a request:
```csharp
public class GetUserQuery : IRequest<UserDto> 
{
    public Guid UserId { get; set; }
}
```

Create a handler:
```csharp
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> HandleAsync(GetUserQuery request, CancellationToken ct)
    {
        // Fetch user logic...
        return new UserDto { Name = "John Doe" };
    }
}
```

## 3. Register Services

In your `Program.cs` or `Startup.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Mediator handlers from the calling assembly
builder.Services.AddMediator();

// (Optional) Register FluentValidation behaviors
builder.Services.AddFluentValidation();
```

## 4. Usage

Inject `IMediator` and send the request:
```csharp
app.MapGet("/users/{id}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.SendAsync(new GetUserQuery { UserId = id });
    return Results.Ok(result);
});
```
