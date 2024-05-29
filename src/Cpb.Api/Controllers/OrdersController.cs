using System.Collections.Immutable;
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
    
    [Roles(Roles.Customer)]
    [HttpGet(DefaultUrl)]
    public async Task<ImmutableList<CustomerCoffeeRecipe>> GetAvailableToOrderRecipes() => 
        await _ordersService.GetAvailableToOrderRecipes(); 
    
    [Roles(Roles.Admin)]
    [HttpGet(DefaultUrl)]
    public async Task<JsonResult<Order, string>> GetOrder(Guid orderId) => 
        JsonResult(await _ordersService.GetOrder(orderId)); 
    
    [Roles(Roles.Customer)]
    [HttpGet(DefaultUrl)]
    public async Task<JsonResult<Order, string>> GetUserOrder() => 
        JsonResult(await _ordersService.GetUserOrder(Actor.UserId)); 
}