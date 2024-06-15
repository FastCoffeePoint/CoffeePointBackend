using System.Text.Json;
using Serilog;

namespace Cpb.Common.Kafka;

public interface IKafkaEventHandler
{ 
    Task HandleRaw(string form);
}

public abstract class KafkaEventHandler<T> : IKafkaEventHandler where T : IEvent
{
    public Task HandleRaw(string form) => Handle(JsonSerializer.Deserialize<T>(form));

    protected abstract Task Handle(T from);

    protected void LogHandlerError(T form, string error) =>
        Log.Error("KAFKA HANDLER {0} ERROR: {1}, DATA: {2}", GetType(), error, form);
}