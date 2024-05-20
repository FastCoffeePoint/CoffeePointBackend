using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Cpb.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

public class OrdersController(OrdersService _ordersService) : CoffeePointController
{
    [Roles(Roles.Customer)]
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> OrderCoffee(OrderCoffeeForm form) => 
        await _ordersService.OrderCoffee(Actor, form);
}