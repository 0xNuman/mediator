using System.Diagnostics;
using Mediator.Abstractions;

namespace Mediator.Extensions.OpenTelemetry;

/// <summary>
/// A pipeline behavior that provides OpenTelemetry distributed tracing for Mediator requests.
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class OpenTelemetryBehaviour<TRequest, TResponse> : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private static readonly ActivitySource ActivitySource = new("Mediator");

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"Mediator {typeof(TRequest).Name}");
        
        if (activity != null)
        {
            activity.SetTag("messaging.mediator.request_type", typeof(TRequest).FullName);
            activity.SetTag("messaging.system", "mediator");
        }

        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.RecordException(ex);
            }
            throw;
        }
    }
}

internal static class ActivityExtensions
{
    public static void RecordException(this Activity activity, Exception ex)
    {
        activity.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            { "exception.type", ex.GetType().FullName },
            { "exception.message", ex.Message },
            { "exception.stacktrace", ex.ToString() }
        }));
    }
}
