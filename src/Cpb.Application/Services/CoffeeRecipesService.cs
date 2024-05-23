using System.Collections.Immutable;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Application.Services;

public class CoffeeRecipesService(DbCoffeePointContext _dc)
{
    public async Task<Result<Guid, string>> LockIngredientForRecipe(Guid orderId, Guid recipeId)
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

        return orderId;
    }
    
    public async Task<ImmutableList<CoffeeRecipe>> GetCoffeeRecipes()
    { 
        var recipes = await _dc.CoffeeRecipes.ExcludeDeleted()
            .AsNoTracking()
            .Include(u => u.Links)
            .ToListAsync();

        var ingredientIds = recipes.SelectMany(u => u.Links.Select(v => v.IngredientId))
            .Distinct()
            .ToList();
        var ingredients = await _dc.Ingredients.ExcludeDeleted()
            .AsNoTracking()
            .Where(u => ingredientIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = recipes.Select(u => 
                new CoffeeRecipe(u.Id, u.Name, u.Links.Select(v => Map(ingredients[v.IngredientId], v)).ToImmutableList()))
            .ToImmutableList();

        return result;
    }

    private CoffeeRecipeIngredient Map(DbIngredient model, DbCoffeeRecipeIngredient link) =>
        new(model.Id, model.Name, link.Amount);
    
    public async Task<Result<Guid, string>> SetIngredientInRecipe(SetIngredientInRecipeForm form)
    {
        var recipeExist = await _dc.CoffeeRecipes.ExcludeDeleted().AnyAsync(u => u.Id == form.RecipeId);
        if (!recipeExist)
            return "The recipe is not found";
        
        var ingredientExist = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Id == form.IngredientId);
        if (!ingredientExist)
            return "The ingredient is not found";

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

        return link.CoffeeRecipeId;
    }
    
    public async Task<Result<Guid, string>> RemoveIngredientFromRecipe(RemoveIngredientFromRecipeForm form)
    {
        var recipeExist = await _dc.CoffeeRecipes.ExcludeDeleted().AnyAsync(u => u.Id == form.RecipeId);
        if (!recipeExist)
            return "The recipe is not found";
        
        var ingredientExist = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Id == form.IngredientId);
        if (!ingredientExist)
            return "The ingredient is not found";

        var link = await _dc.CoffeeRecipeIngredients.FirstOrDefaultAsync(u =>
            u.CoffeeRecipeId == form.RecipeId && u.IngredientId == form.IngredientId);

        if (link == null)
            return form.RecipeId;

        _dc.CoffeeRecipeIngredients.Remove(link);
        await _dc.SaveChangesAsync();

        return link.CoffeeRecipeId;
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

    public async Task<Result<Guid, string>> DeleteCoffeeRecipe(Guid recipeId)
    {
        var recipe = await _dc.CoffeeRecipes.ExcludeDeleted().FirstOrDefaultAsync(u => u.Id == recipeId);
        if (recipe == null)
            return "A recipe is not found";
        if (recipe.IsDeleted)
            return recipeId;

        recipe.MarkDeleted();
        var linkEntities = await _dc.CoffeeRecipeIngredients
            .Where(u => u.CoffeeRecipeId == recipeId)
            .ToListAsync();
        _dc.CoffeeRecipeIngredients.RemoveRange(linkEntities);
        await _dc.SaveChangesAsync();

        return recipeId;
    }

    public async Task<Result<Guid, string>> IncreaseOrderedRecipeCount(Guid recipeId)
    {
        var recipe = await _dc.CoffeeRecipes.FirstOrDefaultAsync(u => u.Id == recipeId);
        if (recipe == null)
            return "Not a single recipe was found";

        recipe.CurrentOrdersCount++;
        await _dc.SaveChangesAsync();

        return recipeId;
    }
    
    public async Task<Result<Guid, string>> DecreaseOrderedRecipeCount(Guid recipeId)
    {
        var recipe = await _dc.CoffeeRecipes.FirstOrDefaultAsync(u => u.Id == recipeId);
        if (recipe == null)
            return "Not a single recipe was found";

        recipe.CurrentOrdersCount--;
        await _dc.SaveChangesAsync();

        return recipeId;
    }
}