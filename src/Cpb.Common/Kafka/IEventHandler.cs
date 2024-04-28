namespace Cpb.Common.Kafka;

public interface IEventHandler<T> where T : IEvent
{
    Task Handle(T appEvent);
}
