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

public class CoffeeIsReadyToBeGottenEventHandler(OrdersService _ordersService) : KafkaEventHandler<CoffeeIsReadyToBeGottenEvent>
{
    public override async Task Handle(CoffeeIsReadyToBeGottenEvent form)
    {
        var result = await _ordersService.MarkOrderAsReadyToBeGotten(form);
        if(result.IsFailure)
            LogHandlerError(form, result.Error);
    }
}

public class OrderHasBeenCompletedEventHandler(OrdersService _ordersService) : KafkaEventHandler<OrderHasBeenCompletedEvent>
{
    public override async Task Handle(OrderHasBeenCompletedEvent form)
    {
        var result = await _ordersService.CompleteOrder(form);
        if(result.IsFailure)
            LogHandlerError(form, result.Error);
    }
}