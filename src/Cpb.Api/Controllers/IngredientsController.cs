using System.Collections.Immutable;
using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Cpb.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

public class IngredientsController(IngredientsService _ingredientsService) : CoffeePointController
{
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> CreateIngredient(CreateIngredientForm form) => 
        await _ingredientsService.Create(form);
    
    [HttpGet(DefaultUrl)]
    public async Task<ImmutableList<Ingredient>> GetIngredients() => 
        await _ingredientsService.GetIngredients();
    
    [HttpDelete(DefaultUrl)]
    public async Task<JsonOptionError> DeleteIngredient(Guid ingredientId) => 
        await _ingredientsService.Delete(ingredientId);
}