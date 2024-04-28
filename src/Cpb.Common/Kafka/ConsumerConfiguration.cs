namespace Cpb.Common.Kafka;

public class ConsumerConfiguration
{
    public List<Topic> Topics { get; set; }
    public string GroupId { get; set; }
    public string BootstrapServers { get; set; }
}