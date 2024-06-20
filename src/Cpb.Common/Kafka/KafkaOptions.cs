namespace Cpb.Common.Kafka;

public class KafkaOptions : IOptions
{
    public static string Name => "KafkaOptions";
    
    public ConsumerConfiguration OrderEventsConsumer { get; set; }
    public ProducerConfiguration OrderEventsProducer { get; set; }
}