using System.Collections.Immutable;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Application.Services;

public class CoffeeMachinesService(DbCoffeePointContext _dc, CoffeeRecipesService _recipesService, IHttpClientFactory _httpFactory)
{
    public async Task<ImmutableList<CoffeeMachine>> GetCoffeeMachines()
    {
        //TODO: rewrite, look at coffee recipes method
        var rawEntities = await _dc.CoffeeMachines
            .ExcludeDeleted()
            .Join(_dc.CoffeeMachineIngredients, 
                recipe => recipe.Id,
                link => link.CoffeeMachineId, 
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

        var mappedList = rawEntities.Select(u => new CoffeeMachine(u.Key, u.First().RecipeName,
                u.Select(v => new CoffeeMachineIngredient(v.IngredientId, v.RecipeName, v.IngredientAmount)).ToImmutableList()))
            .ToImmutableList();
        

        return mappedList;
    }
    
    public async Task<Result> ActualizeIngredientsAmount(Guid machineId, ImmutableList<CoffeeMachineIngredientForm> ingredients)
    {
        var ingredientIds = ingredients.Select(u => u.Id).ToList();
        var entities = await _dc.CoffeeMachineIngredients
            .Where(u => u.CoffeeMachineId == machineId && ingredientIds.Contains(u.IngredientId))
            .ToListAsync();
        
        
        
        return Result.Success();
    }
    

    
    public async Task<Result<Guid, string>> RegisterCoffeeMachine(RegisterCoffeeMachineForm form)
    {
        if (string.IsNullOrEmpty(form.Name) || form.Name.Length > DbCoffeeMachine.NameMaxLength)
            return "Name of the coffee machine is invalid";

        var machine = new DbCoffeeMachine
        {
            Name = form.Name,
            State = CoffeeMachineStates.WaitingApprove,
            MachineHealthCheckEndpointUrl = null,
        }.MarkCreated();
        _dc.CoffeeMachines.Add(machine);
        await _dc.SaveChangesAsync();

        return machine.Id;
    }
    
    public async Task<Result> SetIngredientInMachine(SetIngredientInMachineForm form)
    {
        var recipeExist = await _dc.CoffeeMachines.ExcludeDeleted().AnyAsync(u => u.Id == form.MachineId);
        if (!recipeExist)
            return Result.Failure("The recipe is not found");
        
        var ingredientExist = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Id == form.IngredientId);
        if (!ingredientExist)
            return Result.Failure("The ingredient is not found");

        var link = await _dc.CoffeeMachineIngredients.FirstOrDefaultAsync(u =>
            u.CoffeeMachineId == form.MachineId && u.IngredientId == form.IngredientId);

        if (link == null)
        {
            link = new DbCoffeeMachineIngredient()
            {
                CoffeeMachineId = form.MachineId,
                IngredientId = form.IngredientId,
            };
            _dc.CoffeeMachineIngredients.Add(link);
        }

        link.Amount = form.Amount;
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> RemoveIngredientFromMachine(RemoveIngredientFromMachineForm form)
    {
        var recipeExist = await _dc.CoffeeMachines.ExcludeDeleted().AnyAsync(u => u.Id == form.MachineId);
        if (!recipeExist)
            return Result.Failure("The recipe is not found");
        
        var ingredientExist = await _dc.Ingredients.ExcludeDeleted().AnyAsync(u => u.Id == form.IngredientId);
        if (!ingredientExist)
            return Result.Failure("The ingredient is not found");

        var link = await _dc.CoffeeMachineIngredients.FirstOrDefaultAsync(u =>
            u.CoffeeMachineId == form.MachineId && u.IngredientId == form.IngredientId);

        if (link == null)
            return Result.Success();

        _dc.CoffeeMachineIngredients.Remove(link);
        await _dc.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> ApproveMachine(ApproveCoffeeMachineForm form)
    {
        if (Uri.TryCreate(form.MachineHealthCheckEndpointUrl, UriKind.Absolute, out _))
            return Result.Failure("Uri is invalid");
        
        var machine = await _dc.CoffeeMachines.FirstOrDefaultAsync(u => u.Id == form.MachineId);
        if (machine == null)
            return Result.Failure("The machine not found");

        if (machine.State != CoffeeMachineStates.WaitingApprove)
            return Result.Success();
        
        machine.MachineHealthCheckEndpointUrl = form.MachineHealthCheckEndpointUrl;
        machine.State = CoffeeMachineStates.Active;

        await _dc.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> MakeMachineUnavailable(Guid machineId)
    {
        var machine = await _dc.CoffeeMachines.FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return Result.Failure("The machine not found");

        machine.State = CoffeeMachineStates.Unavailable;
        
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> MakeMachineActive(Guid machineId)
    {
        var machine = await _dc.CoffeeMachines.FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return Result.Failure("The machine not found");

        machine.State = CoffeeMachineStates.Active;
        
        await _dc.SaveChangesAsync();

        return Result.Success();
    }

    public async Task RunCoffeeMachineObservers()
    {
        var machines = await _dc.CoffeeMachines
            .Select(u => new { u.Id, u.MachineHealthCheckEndpointUrl })
            .ToListAsync();

        foreach (var machine in machines)
            BackgroundJob.Enqueue("CoffeeMachineObservers",() => CheckCoffeeMachineHealth(machine.Id, machine.MachineHealthCheckEndpointUrl));
    }

    private async Task CheckCoffeeMachineHealth(Guid id, string url)
    {
        var cancellationToken = new CancellationToken();
        if(Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return; //TODO: logging
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var http = _httpFactory.CreateClient();
            var result = await http.GetAsync(uri);

            if (result.IsSuccessStatusCode)
                await MakeMachineActive(id);
            else
                await MakeMachineUnavailable(id);

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}