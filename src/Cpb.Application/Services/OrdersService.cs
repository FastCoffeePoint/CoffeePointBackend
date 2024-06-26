﻿using System.Collections.Immutable;
using Cpb.Common.Kafka;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Cpb.Application.Services;

public class OrdersService(DbCoffeePointContext _dc, 
    CoffeeMachinesService _machinesService,
    CoffeeRecipesService _recipesService,
    IKafkaProducer _kafkaProducer)
{
    public async Task<ImmutableList<CustomerCoffeeRecipe>> GetAvailableToOrderRecipes()
    {
        var machines = await _machinesService.GetCoffeeMachines();
        var recipes = await _recipesService.GetCoffeeRecipes();
        var lockedRecipes = await _recipesService.GetLockedRecipes();
        var lockedRecipeCounts = lockedRecipes.GroupBy(u => u.RecipeId)
            .Select(u => (RecipeId: u.Key, Count: u.Count()))
            .ToDictionary(u => u.RecipeId);

        var availableRecipes = new List<CustomerCoffeeRecipe>();
        foreach (var recipe in recipes)
        {
            var totalAvailable = 0;
            foreach (var machine in machines)
            {
                List<(int Amount, CoffeeRecipeIngredient Ingredient)> amountByIngredients = recipe.Ingredients.Select(recipeIngredient =>
                {
                    var machineIngredient = machine.Ingredients.FirstOrDefault(k => recipeIngredient.Id == k.Id);
                    if (machineIngredient == null)
                        return (0, recipeIngredient); 
                    
                    var availableIngredients = machineIngredient.Amount / recipeIngredient.Amount;
                    return (availableIngredients, recipeIngredient);
                }).ToList();

                var minAmount = amountByIngredients.MinBy(u => u.Amount);
                totalAvailable += minAmount.Amount;
            }
            
            var exist = lockedRecipeCounts.TryGetValue(recipe.Id, out var locked);
            if (exist)
                totalAvailable -= locked.Count;
                
            availableRecipes.Add(new CustomerCoffeeRecipe(recipe.Id, recipe.Name, totalAvailable, recipe.Ingredients));
        }

        
        return availableRecipes.ToImmutableList();
    }
    
    public async Task<Result<Guid, string>> OrderCoffee(Actor actor, OrderCoffeeForm form)
    {
        // TODO: Write more optimized method for this check.
        var availableRecipes = await GetAvailableToOrderRecipes();
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

        var locking = await _recipesService.LockOrderIngredients(order.Id, order.CoffeeRecipeId);
        if(locking.IsFailure)
            Log.Warning("A order with id {0} was created, but locking ingredients failed.", order.Id);

        var increasing = await _recipesService.IncreaseOrderedRecipeCount(form.RecipeId);
        if(increasing.IsFailure)
            Log.Warning("A order with id {0} was created, but increasing ordered recipe count failed.", order.Id);

        var recipeIngredients = (await _recipesService.GetIngredients(form.RecipeId))
            .Select(u => new OrderedCoffeeIngredientForm(u.Id, u.Amount))
            .ToImmutableList();
        await _kafkaProducer.Push(new CoffeeWasOrderedEvent(order.Id, order.CoffeeRecipeId, recipeIngredients));

        return order.Id;
    }

    public async Task<Result> StartBrewingCoffee(CoffeeStartedBrewingEvent form)
    {
        var order = await _dc.Orders.FirstOrDefaultAsync(u => u.Id == form.OrderId);
        if (order == null)
            return Result.Failure($"The order with id {form.OrderId} is not found");

        if (order.State != OrderStates.InQueue)
            return Result.Failure($"The order({form.OrderId}) state has to be '{OrderStates.InQueue}', but now it's {order.State}");

        order.State = OrderStates.Brewing;
        order.MachineId = form.MachineId;
        
        await _dc.SaveChangesAsync();
        
        //TODO: Here should be a notification to a customer like: Go to a coffee machine, because your coffee will be made in 3 minutes!

        return Result.Success();
    }

    public async Task<Result> MarkOrderAsReadyToBeGotten(CoffeeIsReadyToBeGottenEvent form)
    {
        var order = await _dc.Orders.FirstOrDefaultAsync(u => u.Id == form.OrderId);
        if (order == null)
            return Result.Failure($"The order with id {form.OrderId} is not found");

        if (order.State != OrderStates.Brewing)
            return Result.Failure($"The order({form.OrderId}) state has to be '{OrderStates.Brewing}', but now it's {order.State}");
        
        order.State = OrderStates.ReadyToBeGotten;
        await _dc.SaveChangesAsync();
        
        var actualizingIngredients = await _machinesService.ActualizeIngredientsAmount(form.MachineId, order.CoffeeRecipeId, form.Ingredients);
        var releasingIngredients = await _recipesService.ReleaseOrderIngredient(order.Id);
        var decreasingRecipeCount = await _recipesService.DecreaseOrderedRecipeCount(order.CoffeeRecipeId);

        var result = Result.Combine(actualizingIngredients, releasingIngredients, decreasingRecipeCount);

        return result;
    }

    public async Task<Result> CompleteOrder(Guid orderId)
    {
        var order = await _dc.Orders.FirstOrDefaultAsync(u => u.Id == orderId);
        if (order == null)
            return Result.Failure($"The order with id {orderId} is not found");

        if (order.State != OrderStates.ReadyToBeGotten)
            return Result.Failure($"The order({orderId}) state has to be '{OrderStates.ReadyToBeGotten}', but now it's {order.State}");

        order.State = OrderStates.Complete;
        await _dc.SaveChangesAsync();
        
        return Result.Success();
    }
    
    public async Task<Result> FailOrder(Guid orderId)
    {
        var order = await _dc.Orders.FirstOrDefaultAsync(u => u.Id == orderId);
        if (order == null)
            return Result.Failure($"The order with id {orderId} is not found");

        order.State = OrderStates.Failed;
        await _dc.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Maybe<Order>> GetOrder(Guid orderId)
    {
        var order = await _dc.Orders.ActualReadOnly().FirstOrDefaultAsync(u => u.Id == orderId);
        return Map(order);
    }

    public async Task<Maybe<Order>> GetUserOrder(Guid userId)
    {
        var order = await _dc.Orders.ActualReadOnly().FirstOrDefaultAsync(u => u.UserId == userId);
        return Map(order);
    }

    private Maybe<Order> Map(DbOrder order) =>
        order == null 
            ? Maybe<Order>.None
            : new Order(order.Id, order.State, order.CoffeeRecipeId, order.UserId);
}