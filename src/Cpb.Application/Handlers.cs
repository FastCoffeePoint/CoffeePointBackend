using Cpb.Application.Services;
using Cpb.Common.Kafka;

namespace Cpb.Application;

public class CoffeeStartedBrewingEventHandler(OrdersService _ordersService) : KafkaEventHandler<CoffeeStartedBrewingEvent>
{
    public override async Task Handle(CoffeeStartedBrewingEvent form)
    {
        var result = await _ordersService.StartBrewingCoffee(form);
        if(result.IsFailure)
            LogHandlerError(form, result.Error);
    }
}