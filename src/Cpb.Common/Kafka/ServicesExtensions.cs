using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cpb.Common.Kafka;

public static class ServicesExtensions
{
    public static IServiceCollection AddKafka(this IServiceCollection services)
    {
        services.AddScoped<IKafkaProducer, KafkaProducer>();

        return services;
    }

    public static IServiceCollection AddProducer<TEvent>(this IServiceCollection services, KafkaOptions configuration) where TEvent : class, IEvent
    {
        var configurationFound = configuration.Producers
            .Any(u => u.Topics
                .Any(v => v.Events
                    .Any(j => j == TEvent.Name)));
        if (!configurationFound)
            throw new Exception($"Any producer configuration wasn't registered with name: {TEvent.Name}");
        
        services.AddSingleton<KafkaMessageProducer<TEvent>>();

        return services;
    }
    
    public static IServiceCollection AddConsumer<TEvent, THandler>(this IServiceCollection services, KafkaOptions configuration) 
        where TEvent : class, IEvent
        where THandler : KafkaEventHandler<TEvent>
    {
        var configurationFound = configuration.Consumers
            .Any(u => u.Topics
                .Any(v => v.Events
                    .Any(j => j == TEvent.Name)));
        if (!configurationFound)
            throw new Exception($"Any consumer configuration wasn't registered with name: {TEvent.Name}");
        
        services.AddScoped<KafkaEventHandler<TEvent>, THandler>();
        services.AddHostedService<KafkaConsumerBackgroundJob<TEvent>>();
        return services;
    }
}
