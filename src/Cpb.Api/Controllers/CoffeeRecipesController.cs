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
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> DeleteCoffeeRecipe(Guid recipeId) => 
        await coffeeRecipesService.DeleteCoffeeRecipe(recipeId);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> AddIngredientToRecipe(ManageIngredientInRecipeForm form) => 
        await coffeeRecipesService.AddIngredientToRecipe(form);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> RemoveIngredientFromRecipe(ManageIngredientInRecipeForm form) => 
        await coffeeRecipesService.RemoveIngredientFromRecipe(form); 
    
    [HttpGet(DefaultUrl)]
    public async Task<ImmutableList<CoffeeRecipe>> GetCoffeeRecipes() => 
        await coffeeRecipesService.GetCoffeeRecipes(); 
}