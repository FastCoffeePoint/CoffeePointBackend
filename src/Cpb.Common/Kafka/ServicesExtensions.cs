using Microsoft.Extensions.DependencyInjection;

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
    
    public static IServiceCollection AddConsumer(this IServiceCollection services, ConsumerConfiguration configuration) 
    {
        services.AddHostedService<KafkaConsumerBackgroundJob>(u => new KafkaConsumerBackgroundJob(u, configuration));
        return services;
    }
    
    public static IServiceCollection AddEvent<TEvent, THandler>(this IServiceCollection services)        
        where TEvent : class, IEvent
        where THandler : KafkaEventHandler<TEvent>
    {
        services.AddKeyedScoped<KafkaEventHandler<TEvent>, THandler>(TEvent.Name);
        return services;
    }
}