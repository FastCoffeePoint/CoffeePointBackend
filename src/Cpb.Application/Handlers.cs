﻿using Cpb.Application.Services;
using Cpb.Common.Kafka;

namespace Cpb.Application;

public class CoffeeStartedBrewingEventHandler(OrdersService _ordersService) : KafkaEventHandler<CoffeeStartedBrewingEvent>
{
    protected override async Task Handle(CoffeeStartedBrewingEvent form)
    {
        var result = await _ordersService.StartBrewingCoffee(form);
        if(result.IsSuccess)
            return;
        
        LogHandlerError(form, result.Error);
        var falling = await _ordersService.FailOrder(form.OrderId);
        if(falling.IsFailure)
            LogHandlerError(form, falling.Error);
    }
}

public class CoffeeIsReadyToBeGottenEventHandler(OrdersService _ordersService) : KafkaEventHandler<CoffeeIsReadyToBeGottenEvent>
{
    protected override async Task Handle(CoffeeIsReadyToBeGottenEvent form)
    {
        var result = await _ordersService.MarkOrderAsReadyToBeGotten(form);
        if(result.IsSuccess)
            return;
        
        LogHandlerError(form, result.Error);
        var falling = await _ordersService.FailOrder(form.OrderId);
        if(falling.IsFailure)
            LogHandlerError(form, falling.Error);
    }
}

public class OrderHasBeenCompletedEventHandler(OrdersService _ordersService) : KafkaEventHandler<OrderHasBeenCompletedEvent>
{
    protected override async Task Handle(OrderHasBeenCompletedEvent form)
    {
        var result = await _ordersService.CompleteOrder(form.OrderId);
        if(result.IsSuccess)
            return;
        
        LogHandlerError(form, result.Error);
        var falling = await _ordersService.FailOrder(form.OrderId);
        if(falling.IsFailure)
            LogHandlerError(form, falling.Error);
    }
}

public class OrderHasBeenFailedEventHandler(OrdersService _ordersService) : KafkaEventHandler<OrderHasBeenFailedEvent>
{
    protected override async Task Handle(OrderHasBeenFailedEvent form)
    {
        var falling = await _ordersService.FailOrder(form.OrderId);
        if(falling.IsFailure)
            LogHandlerError(form, falling.Error);
    }
}