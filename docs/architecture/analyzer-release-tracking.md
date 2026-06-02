# Analyzer Release Tracking (RS2000, RS2008)

When building professional Roslyn Analyzers, the build system may report warnings **RS2000** and **RS2008**. These are related to **Diagnostic Release Tracking**, a mechanism used to ensure that diagnostic IDs and severities are tracked across versions to prevent breaking changes for library consumers.

## 🚨 Understanding the Warnings

- **RS2000**: Indicates that a diagnostic rule is not part of any analyzer release.
- **RS2008**: Occurs when analyzer release tracking is not enabled for the project.

For an enterprise-grade library distributed via NuGet, these are important because they prevent you from accidentally changing a diagnostic ID (e.g., from `MED001` to `MED005`) which would break build configurations for your users.

---

## 🛠 How to properly address this in the future

When you are ready to ship the library for production use, follow these steps to enable tracking:

### 1. Create Release Tracking Files
In the `src/Mediator.SourceGenerator/` project root, create two empty markdown files:
- `AnalyzerReleases.Shipped.md`
- `AnalyzerReleases.Unshipped.md`

### 2. Configure the Project File
Add these files as `AdditionalFiles` in your `.csproj`:

```xml
<ItemGroup>
  <AdditionalFiles Include="AnalyzerReleases.Shipped.md" />
  <AdditionalFiles Include="AnalyzerReleases.Unshipped.md" />
</ItemGroup>
```

### 3. Track your first release
In `AnalyzerReleases.Unshipped.md`, add your rules under a version header:

```markdown
## Release 1.0.0

### New Rules
Rule ID | Category | Severity | Notes
------- | -------- | -------- | -----
MED001  | Mediator | Error    | Multiple handlers found
MED002  | Mediator | Warning  | No handler found
```

### 4. Remove Suppressions
Finally, remove the `RS2000` and `RS2008` from the `<NoWarn>` property in the `.csproj`.

---

## ⚖️ Why we suppressed it for now

Currently, the project is in a rapid development phase. Enabling release tracking adds maintenance overhead every time a rule is added or modified. We have suppressed these warnings in the `Mediator.SourceGenerator.csproj` to keep the build output clean until the project reaches a stable `1.0.0` release.
