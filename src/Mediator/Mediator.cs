using System.Collections.Concurrent;
using Mediator.Abstractions;

namespace Mediator;

internal class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), object> _handlerWrappers = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerWrapper = (RequestHandlerWrapper<TResponse>)_handlerWrappers.GetOrAdd(
            (requestType, responseType),
            static key =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(key.RequestType, key.ResponseType);
                return Activator.CreateInstance(wrapperType)!;
            });

        return handlerWrapper.HandleAsync(request, _serviceProvider, cancellationToken);
    }
}