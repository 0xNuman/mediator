using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Mediator.SourceGenerator
{
    [Generator]
    public class MediatorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 1. Discover Handlers
            var registrations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax { BaseList: not null },
                    transform: static (ctx, _) => GetRegistrationInfo(ctx))
                .Where(static m => m is not null);

            // 2. Discover Requests
            var requests = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax or RecordDeclarationSyntax { BaseList: not null },
                    transform: static (ctx, _) => GetRequestInfo(ctx))
                .Where(static m => m is not null);

            var assemblyName = context.CompilationProvider.Select(static (c, _) => c.AssemblyName);
            var options = context.CompilationProvider.Select(static (c, _) => GetOptions(c));

            var combined = registrations.Collect()
                .Combine(requests.Collect())
                .Combine(assemblyName)
                .Combine(options);

            context.RegisterSourceOutput(combined, (ctx, source) => Execute(ctx, source.Left.Left.Left, source.Left.Left.Right, source.Left.Right, source.Right));
        }

        private static string GetOptions(Compilation compilation)
        {
            var attributeSymbol = compilation.GetTypeByMetadataName("Mediator.Abstractions.MediatorOptionsAttribute");
            if (attributeSymbol == null) return "Mediator.Generated";

            var assemblyAttribute = compilation.Assembly.GetAttributes()
                .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

            if (assemblyAttribute == null) return "Mediator.Generated";

            var namespaceArg = assemblyAttribute.NamedArguments
                .FirstOrDefault(kvp => kvp.Key == "Namespace");

            return namespaceArg.Value.Value?.ToString() ?? "Mediator.Generated";
        }

        private static RegistrationInfo? GetRegistrationInfo(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            var model = context.SemanticModel;
            var typeSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

            if (typeSymbol == null || typeSymbol.IsAbstract) return null;

            var registrations = new List<MappingInfo>();

            foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
            {
                if (interfaceSymbol.Name == "IRequestHandler" && interfaceSymbol.TypeArguments.Length == 2)
                {
                    registrations.Add(new MappingInfo(
                        RegistrationType.Request,
                        typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        interfaceSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        interfaceSymbol.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        classDeclaration.GetLocation()
                    ));
                }
                else if (interfaceSymbol.Name == "INotificationHandler" && interfaceSymbol.TypeArguments.Length == 1)
                {
                    registrations.Add(new MappingInfo(
                        RegistrationType.Notification,
                        typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        interfaceSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        null,
                        classDeclaration.GetLocation()
                    ));
                }
            }

            return registrations.Count > 0 ? new RegistrationInfo(registrations) : null;
        }

        private static RequestInfo? GetRequestInfo(GeneratorSyntaxContext context)
        {
            var declaration = (TypeDeclarationSyntax)context.Node;
            var model = context.SemanticModel;
            var typeSymbol = model.GetDeclaredSymbol(declaration) as INamedTypeSymbol;

            if (typeSymbol == null || typeSymbol.IsAbstract) return null;

            foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
            {
                if (interfaceSymbol.Name == "IRequest" && interfaceSymbol.TypeArguments.Length == 1)
                {
                    return new RequestInfo(
                        typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        declaration.GetLocation()
                    );
                }
            }

            return null;
        }

        private static void Execute(
            SourceProductionContext context, 
            ImmutableArray<RegistrationInfo?> registrationsInput, 
            ImmutableArray<RequestInfo?> requestsInput, 
            string? assemblyName, 
            string generatedNamespace)
        {
            var registrations = registrationsInput.Where(r => r != null).SelectMany(r => r!.Mappings).ToList();
            var discoveredRequests = requestsInput.Where(r => r != null).Cast<RequestInfo>().ToList();

            if (assemblyName == "Mediator" || assemblyName == "Mediator.Abstractions") return;

            // --- Validation Phase ---
            
            // MED001: Multiple handlers for same request
            var requestHandlers = registrations.Where(r => r.Type == RegistrationType.Request)
                .GroupBy(r => r.MessageType)
                .Where(g => g.Count() > 1);

            foreach (var group in requestHandlers)
            {
                var handlers = string.Join(", ", group.Select(h => h.HandlerType));
                foreach (var reg in group)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.MultipleHandlersError,
                        reg.Location,
                        group.Key,
                        handlers));
                }
            }

            // MED002: Request has no handler
            var handledRequestTypes = registrations.Where(r => r.Type == RegistrationType.Request).Select(r => r.MessageType).ToImmutableHashSet();
            foreach (var req in discoveredRequests)
            {
                if (!handledRequestTypes.Contains(req.RequestType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.MissingHandlerWarning,
                        req.Location,
                        req.RequestType));
                }
            }

            // --- Generation Phase ---

            var validHandlers = registrations.ToList();

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine("using Mediator.Abstractions;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Threading;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();
            sb.AppendLine("namespace Microsoft.Extensions.DependencyInjection");
            sb.AppendLine("{");
            sb.AppendLine("    public static class MediatorRegistrationExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        public static IMediatorBuilder AddMediator(this IServiceCollection services)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (services.Any(d => d.ServiceType == typeof(IMediator)))");
            sb.AppendLine("                return new MediatorBuilder(services);");
            sb.AppendLine();
            sb.AppendLine($"            services.AddScoped<IMediator, global::{generatedNamespace}.GeneratedMediator>();");
            
            foreach (var reg in validHandlers)
            {
                if (reg.Type == RegistrationType.Request)
                    sb.AppendLine($"            services.AddScoped<IRequestHandler<{reg.MessageType}, {reg.ResponseType}>, {reg.HandlerType}>();");
                else
                    sb.AppendLine($"            services.AddScoped<INotificationHandler<{reg.MessageType}>, {reg.HandlerType}>();");
            }

            sb.AppendLine();
            sb.AppendLine("            return new MediatorBuilder(services);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    internal class MediatorBuilder : IMediatorBuilder");
            sb.AppendLine("    {");
            sb.AppendLine("        public MediatorBuilder(IServiceCollection services) => Services = services;");
            sb.AppendLine("        public IServiceCollection Services { get; }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine($"namespace {generatedNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    internal class GeneratedMediator : IMediator");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IServiceProvider _serviceProvider;");
            sb.AppendLine();
            sb.AppendLine("        public GeneratedMediator(IServiceProvider serviceProvider)");
            sb.AppendLine("        {");
            sb.AppendLine("            _serviceProvider = serviceProvider;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (request == null) throw new ArgumentNullException(nameof(request));");
            sb.AppendLine();

            var requests = validHandlers.Where(r => r.Type == RegistrationType.Request).ToList();
            foreach (var req in requests)
            {
                sb.AppendLine($"            if (request is {req.MessageType} r{validHandlers.IndexOf(req)})");
                sb.AppendLine("            {");
                sb.AppendLine($"                var result = await HandleRequestAsync<{req.MessageType}, {req.ResponseType}>(r{validHandlers.IndexOf(req)}, cancellationToken);");
                sb.AppendLine($"                return (TResponse)(object)result;");
                sb.AppendLine("            }");
            }

            sb.AppendLine();
            sb.AppendLine("            throw new InvalidOperationException($\"No handler registered for request type {request.GetType().FullName}\");");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public async Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (notification == null) throw new ArgumentNullException(nameof(notification));");
            sb.AppendLine();

            var notifications = validHandlers.Where(r => r.Type == RegistrationType.Notification).GroupBy(r => r.MessageType).ToList();
            foreach (var group in notifications)
            {
                var notificationType = group.Key;
                sb.AppendLine($"            if (notification is {notificationType} n{notifications.IndexOf(group)})");
                sb.AppendLine("            {");
                sb.AppendLine($"                await PublishNotificationAsync<{notificationType}>(n{notifications.IndexOf(group)}, cancellationToken);");
                sb.AppendLine("                return;");
                sb.AppendLine("            }");
            }
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private async Task<TResponse> HandleRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest<TResponse>");
            sb.AppendLine("        {");
            sb.AppendLine("            var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();");
            sb.AppendLine("            var behaviors = _serviceProvider.GetServices<IPipelineBehaviour<TRequest, TResponse>>();");
            sb.AppendLine();
            sb.AppendLine("            RequestHandlerDelegate<TResponse> handlerDelegate = () => handler.HandleAsync(request, cancellationToken);");
            sb.AppendLine();
            sb.AppendLine("            var pipeline = behaviors.Aggregate(handlerDelegate, (next, behavior) => () => behavior.Handle(request, next, cancellationToken));");
            sb.AppendLine();
            sb.AppendLine("            return await pipeline();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private async Task PublishNotificationAsync<TNotification>(TNotification notification, CancellationToken cancellationToken) where TNotification : INotification");
            sb.AppendLine("        {");
            sb.AppendLine("            var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();");
            sb.AppendLine("            foreach (var handler in handlers)");
            sb.AppendLine("            {");
            sb.AppendLine("                await handler.HandleAsync(notification, cancellationToken);");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            context.AddSource("Mediator.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private record RegistrationInfo(List<MappingInfo> Mappings);
        private record MappingInfo(RegistrationType Type, string HandlerType, string MessageType, string? ResponseType, Location Location);
        private record RequestInfo(string RequestType, Location Location);
        private enum RegistrationType { Request, Notification }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
