namespace Cpb.Common.Kafka;

public class ConsumerConfiguration
{
    public string Name { get; set; }
    public string Topic { get; set; }
    public string GroupId { get; set; }
    public string BootstrapServers { get; set; }
}