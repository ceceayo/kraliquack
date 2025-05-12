using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using UGS.Shared;

namespace UGS.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IModel _model;
    private List<GameServerWorker> workers;
    private List<Task> tasks;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IDatabase _redis;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IModel model, IHostApplicationLifetime hostApplicationLifetime, IDatabase redis, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _model = model;
        _redis = redis;
        _lifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        workers = new List<GameServerWorker>();
        tasks = new List<Task>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _model.QueueDeclare(queue: "worldsToStart",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        var cts = new CancellationTokenSource();
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SharedUniversalGameServerDataBaseContext>();


        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += (model, ea) =>
        {
            var worldId = int.Parse(Encoding.UTF8.GetString(ea.Body.ToArray()));
            _logger.LogWarning(worldId.ToString());

            var worker = new GameServerWorker(_model, dbContext, _redis, worldId);
            workers.Add(worker);
            tasks.Add(worker.ExecuteAsync(cts.Token));
        };

        _model.BasicConsume(queue: "worldsToStart", autoAck: true, consumer: consumer);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000, stoppingToken);
        }
        

        cts.Cancel();
        await Task.WhenAll(tasks);

    }
}
