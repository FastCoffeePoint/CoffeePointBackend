using System.Collections.Immutable;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Application.Services;

public class CoffeeRecipesService(DbCoffeePointContext _dc)
{
    public async Task<ImmutableList<ConfigurationRecipe>> GetConfigurationRecipes() => await _dc.CoffeeRecipes
        .ActualReadOnly()
        .Select(u => new ConfigurationRecipe(u.Id, "here is your id of a sensor"))
        .ToImmutableListAsync();
    
    public async Task<ImmutableList<CoffeeRecipeIngredient>> GetIngredients(Guid recipeId) => await _dc
        .CoffeeRecipeIngredients
        .AsNoTracking()
        .Include(u => u.CoffeeRecipe)
        .Where(u => u.CoffeeRecipeId == recipeId)
        .Select(u => new CoffeeRecipeIngredient(u.IngredientId, u.CoffeeRecipe.Name, u.Amount))
        .ToImmutableListAsync();
    
    public async Task<Result> LockOrderIngredients(Guid orderId, Guid recipeId)
    {
        var ingredients = await _dc.CoffeeRecipeIngredients
            .AsNoTracking()
            .Where(u => u.CoffeeRecipeId == recipeId)
            .ToListAsync();

        var lockedIngredients = ingredients
            .Select(u => new DbLockedIngredient
            {
                OrderId = orderId, 
                IngredientId = u.IngredientId, 
                LockedAmount = u.Amount,
                RecipeId = recipeId,
            }.MarkCreated())
            .ToList();

        _dc.LockedIngredients.AddRange(lockedIngredients);
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> ReleaseOrderIngredient(Guid orderId)
    {
        var lockedIngredients = await _dc.LockedIngredients
            .Where(u => u.OrderId == orderId)
            .ToListAsync();

        foreach (var ingredient in lockedIngredients)
            ingredient.MarkDeleted();
        
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<ImmutableList<CoffeeRecipe>> GetCoffeeRecipes()
    { 
        var recipes = await _dc.CoffeeRecipes.ActualReadOnly()
            .Include(u => u.Links)
            .ToListAsync();

        var ingredientIds = recipes.SelectMany(u => u.Links.Select(v => v.IngredientId))
            .Distinct()
            .ToList();
        var ingredients = await _dc.Ingredients.ActualReadOnly()
            .Where(u => ingredientIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = recipes.Select(u => 
                new CoffeeRecipe(u.Id, u.Name, u.Links.Select(v => Map(ingredients[v.IngredientId], v)).ToImmutableList()))
            .ToImmutableList();

        return result;
    }

    private CoffeeRecipeIngredient Map(DbIngredient model, DbCoffeeRecipeIngredient link) =>
        new(model.Id, model.Name, link.Amount);
    
    public async Task<Result> SetIngredientInRecipe(SetIngredientInRecipeForm form)
    {
        var recipeExist = await _dc.CoffeeRecipes.ExcludeDeleted().AnyAsync(u => u.Id == form.RecipeId);
        if (!recipeExist)
            return Result.Failure("The recipe is not found");
        
        var ingredientExist = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Id == form.IngredientId);
        if (!ingredientExist)
            return Result.Failure("The ingredient is not found");

        var link = await _dc.CoffeeRecipeIngredients.FirstOrDefaultAsync(u =>
            u.CoffeeRecipeId == form.RecipeId && u.IngredientId == form.IngredientId);

        if (link == null)
        {
            link = new DbCoffeeRecipeIngredient
            {
                CoffeeRecipeId = form.RecipeId,
                IngredientId = form.IngredientId,
            };
            _dc.CoffeeRecipeIngredients.Add(link);
        }

        link.Amount = form.Amount;
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> RemoveIngredientFromRecipe(RemoveIngredientFromRecipeForm form)
    {
        var recipeExist = await _dc.CoffeeRecipes.ExcludeDeleted().AnyAsync(u => u.Id == form.RecipeId);
        if (!recipeExist)
            return Result.Failure("The recipe is not found");
        
        var ingredientExist = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Id == form.IngredientId);
        if (!ingredientExist)
            return Result.Failure("The ingredient is not found");

        var link = await _dc.CoffeeRecipeIngredients.FirstOrDefaultAsync(u =>
            u.CoffeeRecipeId == form.RecipeId && u.IngredientId == form.IngredientId);

        if (link == null)
            return Result.Success();

        _dc.CoffeeRecipeIngredients.Remove(link);
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result<Guid, string>> CreateCoffeeRecipe(CreateCoffeeRecipe form)
    {
        var isNameBusy = await _dc.CoffeeRecipes.ExcludeDeleted()
            .AnyAsync(u => u.DeleteDate == null && u.Name == form.Name);

        if (isNameBusy)
            return "The coffee name is busy";

        var coffeeRecipe = new DbCoffeeRecipe
        {
            Id = Guid.NewGuid(),
            Name = form.Name
        }.MarkCreated();
        
        _dc.CoffeeRecipes.Add(coffeeRecipe);
        await _dc.SaveChangesAsync();

        return coffeeRecipe.Id;
    }

    public async Task<ImmutableList<LockedRecipe>> GetLockedRecipes() => await _dc.LockedIngredients
        .ExcludeDeleted()
        .GroupBy(u => new { u.RecipeId, u.OrderId })
        .Select(u => new LockedRecipe(u.Key.OrderId, u.Key.RecipeId))
        .ToImmutableListAsync();

    public async Task<Result> DeleteCoffeeRecipe(Guid recipeId)
    {
        var recipe = await _dc.CoffeeRecipes.ExcludeDeleted().FirstOrDefaultAsync(u => u.Id == recipeId);
        if (recipe == null)
            return Result.Failure("A recipe is not found");
        if (recipe.IsDeleted)
            return Result.Success();

        recipe.MarkDeleted();
        var linkEntities = await _dc.CoffeeRecipeIngredients
            .Where(u => u.CoffeeRecipeId == recipeId)
            .ToListAsync();
        _dc.CoffeeRecipeIngredients.RemoveRange(linkEntities);
        await _dc.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> IncreaseOrderedRecipeCount(Guid recipeId)
    {
        var recipe = await _dc.CoffeeRecipes.ActualReadOnly().FirstOrDefaultAsync(u => u.Id == recipeId);
        if (recipe == null)
            return Result.Failure("Not a single recipe was found");

        recipe.CurrentOrdersCount++;
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> DecreaseOrderedRecipeCount(Guid recipeId)
    {
        var recipe = await _dc.CoffeeRecipes.ActualReadOnly().FirstOrDefaultAsync(u => u.Id == recipeId);
        if (recipe == null)
            return Result.Failure("Not a single recipe was found");

        recipe.CurrentOrdersCount--;
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
}