namespace Cpb.Common.Kafka;

public class KafkaOptions : IOptions
{
    public static string Name => "KafkaOptions";
    
    public ConsumerConfiguration OrderEventsConsumer { get; set; }
    public List<ProducerConfiguration> Producers { get; set; }
}