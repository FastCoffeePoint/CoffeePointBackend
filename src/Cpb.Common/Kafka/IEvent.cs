namespace Cpb.Common.Kafka;

public interface IEvent
{
    static abstract string Name { get; }
}