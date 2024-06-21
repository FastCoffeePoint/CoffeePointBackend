using System.Text;
using Confluent.Kafka;
using CSharpFunctionalExtensions;
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
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
            .SetKeyDeserializer(new DefaultJsonDeserializer<Ignore>())
            .Build();
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
            var eventName = Maybe<string>.None;
            ConsumeResult<Ignore, string> consumeResult = null;
            try
            {
                Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} is consuming.",_consumer.MemberId, _topic);
                consumeResult = _consumer.Consume(stoppingToken);
                
                eventName = GetValue(consumeResult.Message.Headers, IEvent.HeaderName);
                if (eventName.HasNoValue)
                {
                    Log.Warning("KAFKA CONSUMER{0}: A message equals null, topic - {1}", 
                        _consumer.MemberId,_topic);
                    continue;
                }
                
                var appEvent = consumeResult.Message.Value;
                Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} have received a message: {2}, event - {3}",
                    _consumer.MemberId, _topic, appEvent, eventName.Value);
                if (appEvent == null)
                {
                    Log.Warning("KAFKA CONSUMER{0}: A message equals null, topic - {1}, event - {2}", 
                        _consumer.MemberId,_topic, eventName.Value);
                    continue;
                }
                
                await using var scope = _serviceProvider.CreateAsyncScope();
                var handler = scope.ServiceProvider.GetRequiredKeyedService<IKafkaEventHandler>(eventName.Value);
                if (handler == null)
                    throw new Exception($"Event handler for {eventName.Value} wasn't registered");

                await handler.HandleRaw(appEvent);
            }
            catch (Exception e)
            {
                Log.Error(e, "KAFKA CONSUMER{0}: message can't be handled because of an exception, topic - {1}, event - {2}, message - {3}", 
                    _consumer.MemberId, _topic, eventName.GetValueOrDefault("null"), consumeResult?.Message.Value ?? "null");
            }
        }

        Log.Information("KAFKA CONSUMER{0}: A consumer for the topic {1} is closing.", _consumer.MemberId, _topic);
        _consumer.Close();
    }

    private Maybe<string> GetValue(Headers headers, string key)
    {
        var header = headers.FirstOrDefault(u => u.Key == key);
        if(header == null)
            return Maybe<string>.None;

        var value = Encoding.UTF8.GetString(header.GetValueBytes());
        return value;
    }
}