namespace Cpb.Common.Kafka;

public class ProducerConfiguration
{
    public string Topic { get; set; }
    public string BootstrapServers { get; set; }
    public string Audience { get; set; }
}