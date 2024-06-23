namespace Cpb.Common.Kafka;

public interface IEvent
{
    public static string EventTypeHeaderName = "EventType";
    public static string AudienceHeaderName = "Audience";
    
    static abstract string Name { get; }
}