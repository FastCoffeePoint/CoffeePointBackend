using System.Collections.Immutable;
using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Cpb.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

public class CoffeeRecipesController(CoffeeRecipesService coffeeRecipesService) : CoffeePointController
{
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> CreateCoffeeRecipe(CreateCoffeeRecipe form) => 
        await coffeeRecipesService.CreateCoffeeRecipe(form);
    
    [HttpDelete(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> DeleteCoffeeRecipe(Guid recipeId) => 
        await coffeeRecipesService.DeleteCoffeeRecipe(recipeId);
    
    [HttpPut(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> SetIngredientInRecipe(SetIngredientInRecipeForm setIngredientInRecipeForm) => 
        await coffeeRecipesService.SetIngredientInRecipe(setIngredientInRecipeForm);
    
    [HttpDelete(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> RemoveIngredientFromRecipe(RemoveIngredientFromRecipeForm form) => 
        await coffeeRecipesService.RemoveIngredientFromRecipe(form); 
    
    [HttpGet(DefaultUrl)]
    public async Task<ImmutableList<CoffeeRecipe>> GetCoffeeRecipes() => 
        await coffeeRecipesService.GetCoffeeRecipes(); 
    
    [HttpGet(DefaultUrl)]
    public async Task<ImmutableList<CustomerCoffeeRecipe>> GetAvailableToOrderRecipes() => 
        await coffeeRecipesService.GetAvailableToOrderRecipes(); 
}