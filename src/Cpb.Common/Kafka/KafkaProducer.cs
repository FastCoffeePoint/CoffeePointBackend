using Microsoft.Extensions.DependencyInjection;

namespace Cpb.Common.Kafka;

public interface IKafkaProducer
{
    Task Push<T>(T appEvent) where T : class, IEvent;
}

public class KafkaProducer : IKafkaProducer
{
    private readonly IServiceProvider _serviceProvider;

    public KafkaProducer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Push<T>(T appEvent) where T : class, IEvent
    {
        var producer = _serviceProvider.GetService<KafkaMessageProducer<T>>();
        if (producer == null)
            throw new Exception($"Producer for event {nameof(T)} wasn't registered");

        await producer.Push(appEvent);
    }
}
