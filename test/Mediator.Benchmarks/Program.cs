using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Mediator.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<MediatorBenchmarks>();
    }
}

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private IServiceProvider _serviceProvider = null!;
    private IMediator _mediator = null!;
    private BenchmarkRequest _request = null!;
    private IRequestHandler<BenchmarkRequest, string> _handler = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddScoped<IRequestHandler<BenchmarkRequest, string>, BenchmarkRequestHandler>();

        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
        _request = new BenchmarkRequest { Message = "Benchmark" };
        _handler = _serviceProvider.GetRequiredService<IRequestHandler<BenchmarkRequest, string>>();
    }

    [Benchmark(Baseline = true)]
    public Task<string> DirectCall()
    {
        return _handler.HandleAsync(_request);
    }

    [Benchmark]
    public Task<string> MediatorSend()
    {
        return _mediator.SendAsync(_request);
    }
}

public class BenchmarkRequest : IRequest<string>
{
    public string Message { get; init; } = string.Empty;
}

public class BenchmarkRequestHandler : IRequestHandler<BenchmarkRequest, string>
{
    public Task<string> HandleAsync(BenchmarkRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(request.Message);
    }
}