using BuildingBlocks.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging.BackgroundServices;

public class ConsumerBackgroundWorker : BackgroundService
{
    private readonly IEnumerable<IBusSubscriber> _subscribers;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ConsumerBackgroundWorker> _logger;

    public ConsumerBackgroundWorker(
        IEnumerable<IBusSubscriber> subscribers,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ConsumerBackgroundWorker> logger)
    {
        _subscribers = subscribers;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = _subscribers.Select(s => s.StartAsync(stoppingToken));
        var combinedTask = Task.WhenAll(tasks);

        return combinedTask;
    }

}