using Serilog;

namespace Cpb.Common.Kafka;

public abstract class KafkaEventHandler<T> where T : IEvent
{
    public abstract Task Handle(T form);

    protected void LogHandlerError(T form, string error) =>
        Log.Error("KAFKA HANDLER {0} ERROR: {1}, DATA: {2}", GetType(), error, form);
}
