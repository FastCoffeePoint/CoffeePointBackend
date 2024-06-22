using Microsoft.Extensions.DependencyInjection;

namespace Cpb.Common.Kafka;

public static class ServicesExtensions
{
    //Don't support multiple topics now
    public static IServiceCollection AddProducer(this IServiceCollection services, ProducerConfiguration configuration)
    {
        services.AddSingleton<IKafkaProducer, KafkaProducer>(u => new KafkaProducer(configuration));

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
        services.AddKeyedScoped<IKafkaEventHandler, THandler>(TEvent.Name);
        return services;
    }
}