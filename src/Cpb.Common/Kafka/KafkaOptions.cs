namespace Cpb.Common.Kafka;

public class KafkaOptions
{
    public const string Name = "KafkaOptions";
    
    public List<ConsumerConfiguration> Consumers { get; set; }
    public List<ProducerConfiguration> Producers { get; set; }
}