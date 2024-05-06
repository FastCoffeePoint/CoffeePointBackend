using System.Collections.Immutable;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Application.Services;

public class CoffeeRecipesService(DbCoffeePointContext _dc)
{
    public async Task<ImmutableList<CoffeeRecipe>> GetCoffeeRecipes()
    { 
        var rawEntities = await _dc.CoffeeRecipes
            .Join(_dc.CoffeeRecipeIngredients, 
                recipe => recipe.Id,
                link => link.CoffeeRecipeId, 
                (recipe, link) => new { Recipe = recipe, Link = link }
            )
            .Join(_dc.Ingredients, 
                recipeAndLink => recipeAndLink.Link.IngredientId, 
                ingredient => ingredient.Id, 
                (recipeAndLink, ingredient) => new 
                {
                    RecipeId = recipeAndLink.Recipe.Id,
                    RecipeName = recipeAndLink.Recipe.Name,
                    
                    IngredientId = ingredient.Id,
                    IngredientAmount = recipeAndLink.Link.Amount,
                    IngredientName = ingredient.Name,
                }
            ).GroupBy(u => u.RecipeId)
            .ToListAsync();

        var mappedList = rawEntities.Select(u => new CoffeeRecipe(u.Key, u.First().RecipeName,
                u.Select(v => new CoffeeRecipeIngredient(v.IngredientId, v.RecipeName, v.IngredientAmount)).ToImmutableList()))
            .ToImmutableList();
        

        return mappedList;
    }
    
    public async Task<Result<Guid, string>> AddIngredientToRecipe(ManageIngredientInRecipeForm form)
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
                Amount = 1,
            };
            _dc.CoffeeRecipeIngredients.Add(link);
            await _dc.SaveChangesAsync();
            return link.CoffeeRecipeId;
        }

        link.Amount++;
        await _dc.SaveChangesAsync();

        return link.CoffeeRecipeId;
    }
    
    public async Task<Result<Guid, string>> RemoveIngredientFromRecipe(ManageIngredientInRecipeForm form)
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

        if (link.Amount == 0)
            return link.CoffeeRecipeId;
        
        link.Amount--;
        
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
}