# Configuration with MediatorOptions

While the Mediator library works out-of-the-box with sensible defaults, enterprise projects often require fine-grained control over where and how code is generated. This is managed via the `MediatorOptionsAttribute`.

## 🛠 Usage

To customize the Mediator's behavior, add the following attribute to any C# file in your project (typically `Program.cs` or `AssemblyInfo.cs`):

```csharp
using Mediator.Abstractions;

[assembly: MediatorOptions(Namespace = "MyCompany.Messaging.Generated")]
```

## 🏗 Key Use Cases

### 1. Naming Convention Alignment
Large organizations often have strict rules about namespace structures (e.g., `CompanyName.Division.Project.Area`). The `MediatorOptions` attribute allows you to align the generated dispatcher with your internal standards.

### 2. Isolation in Large Monoliths
In a solution with hundreds of projects, you may want to ensure that the generated Mediator doesn't clutter the root `Mediator` namespace. By specifying a custom namespace, you isolate the generated artifacts:
- `MyProject.Domain` (Your code)
- `MyProject.Domain.Internal.Mediator` (Generated code)

### 3. Avoiding Naming Conflicts
If your project already uses a class named `GeneratedMediator` for another purpose, you can move the library's version to a different namespace to eliminate ambiguity without changing your own code.

## ⚙️ How it Works

The Source Generator scans your assembly metadata during the compilation process:
1.  **Detection**: It looks for the `[assembly: MediatorOptions]` attribute.
2.  **Extraction**: It reads the `Namespace` property.
3.  **Generation**: It writes the `GeneratedMediator` class into that namespace and updates the `AddMediator()` extension method to reference it correctly using `global::` aliases.

If the attribute is omitted, the library defaults to `Mediator.Generated`.
