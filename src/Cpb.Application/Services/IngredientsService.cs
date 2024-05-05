using System.Collections.Immutable;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Application.Services;

public class IngredientsService(DbCoffeePointContext _dc)
{
    public async Task<ImmutableList<Ingredient>> GetIngredients() => await _dc.Ingredients.ExcludeDeleted()
        .AsNoTracking()
        .Select(u => new Ingredient(u.Id, u.Name))
        .ToImmutableListAsync();

    public async Task<Result<Guid, string>> Create(CreateIngredientForm form)
    {
        if (string.IsNullOrEmpty(form.Name) || form.Name.Length > 3)
            return "Invalid name for ingredient";
        
        var nameIsBusy = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Name == form.Name);
        if (nameIsBusy)
            return "The ingredient name is busy";

        var ingredient = new DbIngredient
        {
            Id = Guid.NewGuid(), 
            Name = form.Name
        }.MarkCreated();
        _dc.Ingredients.Add(ingredient);
        await _dc.SaveChangesAsync();

        return ingredient.Id;
    }

    public async Task<Result<Guid, string>> Delete(Guid ingredientId)
    {
        var ingredient = await _dc.Ingredients.FirstOrDefaultAsync(u => u.Id == ingredientId);
        if (ingredient == null)
            return "A ingredient is not found";
        if (ingredient.IsDeleted)
            return ingredientId;

        var anyCoffeeRecipeHasIngredient =  await _dc.CoffeeRecipeIngredients.AnyAsync(u => u.IngredientId == ingredientId);
        if (anyCoffeeRecipeHasIngredient)
            return "Any coffee recipe has the ingredient, so that you can't delete this.";

        ingredient.MarkDeleted();
        await _dc.SaveChangesAsync();

        return ingredientId;
    }
}