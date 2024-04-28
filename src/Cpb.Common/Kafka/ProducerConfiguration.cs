namespace Cpb.Common.Kafka;

public class ProducerConfiguration
{
    public List<Topic> Topics { get; set; }
    
    public string BootstrapServers { get; init; }
}