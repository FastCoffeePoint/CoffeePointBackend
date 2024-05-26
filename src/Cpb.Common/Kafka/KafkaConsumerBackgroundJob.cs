using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Cpb.Common.Kafka;

public class KafkaConsumerBackgroundJob<TEvent> : BackgroundService where TEvent : class, IEvent
{
    private readonly IConsumer<Null, TEvent> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _topic;

    public KafkaConsumerBackgroundJob(IOptionsMonitor<KafkaOptions> configuration,
        IServiceProvider serviceProvider)
    {
        var consumerConfiguration = configuration.CurrentValue.Consumers
            .FirstOrDefault(u => u.Topics
                .Any(v => v.Events
                    .Any(j => j == TEvent.Name)))
            ?? throw new Exception($"Consumer configuration for the event {TEvent.Name} wasn't registered");

        var topic = consumerConfiguration.Topics.FirstOrDefault(u => u.Events.Any(v => v == TEvent.Name));
        _topic = topic?.Name  ?? throw new Exception($"Consumer configuration for the event {TEvent.Name} wasn't registered");
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = consumerConfiguration.BootstrapServers,
            GroupId = consumerConfiguration.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
        };
        
        _consumer = new ConsumerBuilder<Null, TEvent>(consumerConfig)
                    .SetValueDeserializer(new DefaultJsonDeserializer<TEvent>())
                    .Build();
        
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //TODO: looks ugly, but BackgroundService locks the app, because we have _consumer.Consume(...) doesn't hava an async version.
        // Perhaps I should user Hangfire and this call won't fuck my mind at all.
        Task.Run(() => Process(stoppingToken), stoppingToken); 
        return Task.CompletedTask;
    }
    
    private async void Process(CancellationToken stoppingToken)
    {
        Log.Information("KAFKA CONSUMER: A consumer for {0} is starting.", TEvent.Name);
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<Null, TEvent> consumeResult = null;
            try
            {
                Log.Information("KAFKA CONSUMER: A consumer for {0} is consuming.", TEvent.Name);
                consumeResult = _consumer.Consume(stoppingToken);
                
                var appEvent = consumeResult.Message.Value;
                Log.Information("KAFKA CONSUMER: A consumer for {0} have received a message: {1}.", TEvent.Name, appEvent);
                if (appEvent == null)
                {
                    Log.Warning("PERSON KAFKA CONSUMER ERROR: A message equals null, topic - {0}, message type - {1}", 
                        _topic, typeof(TEvent).ToString());
                    continue;
                }
                
                var handler = _serviceProvider.GetService<KafkaEventHandler<TEvent>>();
                if (handler == null)
                    throw new Exception($"Event handler for {typeof(TEvent)} wasn't registered");

                await handler.Handle(appEvent);
            }
            catch (Exception e)
            {
                Log.Error(e, "PERSON KAFKA CONSUMER ERROR: message can't be handled because of an exception, topic - {0}, message type - {1}, message - {2}", 
                    _topic, typeof(TEvent).ToString(), consumeResult?.Message);
            }
        }

        _consumer.Close();
    }
}
