using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cpb.Application.Services;

public class OrdersService(DbCoffeePointContext _dc, CoffeeRecipesService _recipesService)
{
    public async Task<Result<Guid, string>> OrderCoffee(Actor actor, OrderCoffeeForm form)
    {
        // TODO: Write more optimized method for this check.
        var availableRecipes = await _recipesService.GetAvailableToOrderRecipes();
        var recipeInAvailable = availableRecipes.Any(u => u.Id == form.RecipeId && u.AvailableAmount > 0);
        if (!recipeInAvailable)
            return "Sorry, the ingredients for the chosen recipe is out of stock. Try something another.";

        var order = new DbOrder
        {
            Id = Guid.NewGuid(),
            CoffeeRecipeId = form.RecipeId,
            UserId = actor.UserId,
        }.MarkCreated();
        _dc.Orders.Add(order);
        await _dc.SaveChangesAsync();

        var increasing = await _recipesService.IncreaseOrderedRecipeCount(form.RecipeId);
        if(increasing.IsFailure)
            Log.Warning("A order with id {0} was created, but increasing ordered recipe count failed.", order.Id);

        return order.Id;
    }

    public async Task<Result<Guid, string>> StartBrewingCoffee(CoffeeStartedBrewingEvent form)
    {
        var order = await _dc.Orders.FirstOrDefaultAsync(u => u.Id == form.OrderId);
        if (order == null)
            return $"The order with id {form.OrderId} is not found";

        if (order.State != OrderStates.InOrder)
            return $"The order({form.OrderId}) state has to be '{OrderStates.InOrder}', but now it's {order.State}";

        order.State = OrderStates.IsBrewing;
        await _dc.SaveChangesAsync();

        return order.Id;
    }
}