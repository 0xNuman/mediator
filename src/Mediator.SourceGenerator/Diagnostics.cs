using Microsoft.CodeAnalysis;

namespace Mediator.SourceGenerator
{
    internal static class Diagnostics
    {
        public static readonly DiagnosticDescriptor MultipleHandlersError = new DiagnosticDescriptor(
            id: "MED001",
            title: "Multiple handlers found for the same request",
            messageFormat: "Request '{0}' has multiple handlers registered: {1}. Only one handler is allowed per request.",
            category: "Mediator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly DiagnosticDescriptor MissingHandlerWarning = new DiagnosticDescriptor(
            id: "MED002",
            title: "No handler found for request",
            messageFormat: "Request '{0}' does not have a registered handler. Sending this request will result in a runtime exception.",
            category: "Mediator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);
    }
}
