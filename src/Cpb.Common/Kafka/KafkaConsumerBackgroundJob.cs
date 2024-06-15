using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Cpb.Common.Kafka;

public class KafkaConsumerBackgroundJob : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _topic;

    public KafkaConsumerBackgroundJob(IServiceProvider serviceProvider,
        ConsumerConfiguration consumerConfiguration)
    {
        _topic = consumerConfiguration.Topic;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = consumerConfiguration.BootstrapServers,
            GroupId = consumerConfiguration.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
        };

        _serviceProvider = serviceProvider;
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
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
        Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} is starting.", _consumer.MemberId, _topic);
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<Ignore, string> consumeResult = null;
            try
            {
                Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} is consuming.",_consumer.MemberId, _topic);
                consumeResult = _consumer.Consume(stoppingToken);
                
                var appEvent = consumeResult.Message.Value;
                Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} have received a message: {2}.",_consumer.MemberId, _topic, appEvent);
                if (appEvent == null)
                {
                    Log.Warning("KAFKA CONSUMER{0}: A message equals null, topic - {1}", _consumer.MemberId,_topic);
                    continue;
                }

                var eventName = consumeResult.Message.Headers.FirstOrDefault(u => u.Key == IEvent.HeaderName);
                if (eventName?.ToString() == null)
                {
                    Log.Warning("KAFKA CONSUMER{0}: A message equals null, topic - {1}", _consumer.MemberId,_topic);
                    continue;
                }
                
                await using var scope = _serviceProvider.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IKafkaEventHandler>(eventName.ToString());
                if (handler == null)
                    throw new Exception($"Event handler for {eventName} wasn't registered");

                await handler.HandleRaw(appEvent);
            }
            catch (Exception e)
            {
                Log.Error(e, "KAFKA CONSUMER{0}: message can't be handled because of an exception, topic - {1}, message - {2}", 
                    _consumer.MemberId, _topic, consumeResult?.Message.Value ?? "null");
            }
        }

        Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} is closing.", _consumer.MemberId, _topic);
        _consumer.Close();
    }
}
