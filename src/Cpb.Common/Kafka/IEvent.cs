namespace Cpb.Common.Kafka;

public interface IEvent
{
    public static string HeaderName = "EventName";
    
    static abstract string Name { get; }
}