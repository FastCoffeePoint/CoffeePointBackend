using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Cpb.Common.Kafka;

public class KafkaMessageProducer<TEvent> where TEvent : class, IEvent
{
    private readonly ProducerConfiguration _producerConfiguration;
    private readonly IProducer<Null, TEvent> _producer;
    private readonly string _topic;

    public KafkaMessageProducer(IOptionsMonitor<KafkaOptions> configuration)
    {
        var producerConfiguration = configuration.CurrentValue.Producers
            .FirstOrDefault(u => u.Topics
                .Any(v => v.Events
                    .Any(j => j == TEvent.Name)));
        _producerConfiguration = producerConfiguration ?? throw new Exception($"A producer configuration for the event {TEvent.Name} wasn't registered");

        var topic = _producerConfiguration.Topics.FirstOrDefault(u => u.Events.Any(v => v == TEvent.Name));
        _topic = topic?.Name  ?? throw new Exception($"Consumer configuration for the event {TEvent.Name} wasn't registered");
        
        var producerConfig = new ProducerConfig
        { BootstrapServers = _producerConfiguration.BootstrapServers };

        _producer = new ProducerBuilder<Null, TEvent>(producerConfig)
            .SetValueSerializer(new DefaultJsonSerializer<TEvent>())
            .Build();
    }
    
    public async Task Push(TEvent appEvent)
    {
        var kafkaMessage = new Message<Null, TEvent>
        { Value = appEvent };

        await _producer.ProduceAsync(_topic, kafkaMessage);
    }
}