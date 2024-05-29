using System.Collections.Immutable;
using Cpb.Application.Configurations;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace Cpb.Application.Services;

public class CoffeeMachinesService(DbCoffeePointContext _dc, 
    IOptionsMonitor<BusinessOptions> _businessOptions,
    CoffeeRecipesService _recipesService,
    IHttpClientFactory _httpFactory)
{
    public async Task<ImmutableList<CoffeeMachine>> GetCoffeeMachines()
    {
        var machines = await _dc.CoffeeMachines.ActualReadOnly()
            .Include(u => u.Links)
            .ToListAsync();

        var machineIds = machines.SelectMany(u => u.Links.Select(v => v.IngredientId))
            .Distinct()
            .ToList();
        var ingredients = await _dc.Ingredients.ActualReadOnly()
            .Where(u => machineIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = machines.Select(u => 
                new CoffeeMachine(u.Id, u.Name, u.Links.Select(v => Map(ingredients[v.IngredientId], v)).ToImmutableList()))
            .ToImmutableList();

        return result;
    }
    
    private CoffeeMachineIngredient Map(DbIngredient model, DbCoffeeMachineIngredient link) =>
        new(model.Id, model.Name, link.Amount);
    
    public async Task<Result> ActualizeIngredientsAmount(Guid machineId, Guid recipeId, ImmutableList<CoffeeMachineIngredientForm> realIngredients)
    {
        var ingredientIds = realIngredients.Select(u => u.Id).ToList();
        var virtualIngredients = await _dc.CoffeeMachineIngredients
            .ExcludeDeleted()
            .Where(u => u.CoffeeMachineId == machineId && ingredientIds.Contains(u.IngredientId))
            .ToDictionaryAsync(u => u.IngredientId);
        var recipeIngredients = (await _recipesService.GetIngredients(recipeId))
            .ToDictionary(u => u.Id);
        

        foreach (var realIngredient in realIngredients)
        {
            var virtualIngredientExist = virtualIngredients.TryGetValue(realIngredient.Id, out var virtualIngredient);
            if (!virtualIngredientExist)
            {
                Log.Error("ACTUALIZING INGREDIENTS: A ingredient with a id {0} in a coffee machine with a id {1} can't be found in our database.",
                    realIngredient.Id, machineId);
                continue;
            }
            var recipeIngredientExist = recipeIngredients.TryGetValue(realIngredient.Id, out var recipeIngredient);
            if (!recipeIngredientExist)
            {
                Log.Error("ACTUALIZING INGREDIENTS: A ingredient with a id {0} in a recipe with a id {} can't be found in our database.",
                    realIngredient.Id, recipeId);
                continue;
            }
            
            virtualIngredient.Amount = realIngredient.AmountAfterExecution;
            
            var wasRefillingIngredients = realIngredient.AmountAfterExecution - realIngredient.AmountBeforeExecution > 0;
            if (wasRefillingIngredients)
                continue;

            var spendAmount = realIngredient.AmountBeforeExecution - realIngredient.AmountAfterExecution;
            var expectedAmount = recipeIngredient.Amount;
            var differance = Convert.ToDouble(Math.Abs(spendAmount - expectedAmount));
            var missingPercent = differance / expectedAmount * 100d;
            var tolerancePercent = _businessOptions.CurrentValue.IngredientMissingTolerancePercent;

            //TODO: maybe later I'll do autocorrecting for expected amount in a recipe ingredient.  
            if (missingPercent > tolerancePercent)
                Log.Error("ACTUALIZING INGREDIENTS: A ingredient with a id {0} in a coffee machine with a id {1} has acrossed the tolerance percent with value {2}% ",
                    realIngredient.Id, machineId, missingPercent);
        }

        await _dc.SaveChangesAsync();
        
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

    public async Task<Result<CoffeeMachineConfiguration, string>> GetConfiguration(Guid machineId)
    {
        var machine = await _dc.CoffeeMachines.ExcludeDeleted().FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return "The machine not found";

        var ingredients = await _dc.CoffeeMachineIngredients.ActualReadOnly()
            .Where(u => u.CoffeeMachineId == machineId)
            .Select(u => new ConfigurationIngredient(u.IngredientId, "here is your id of a sensor"))
            .ToImmutableListAsync();

        var recipes = await _recipesService.GetConfigurationRecipes();

        var configuration = new CoffeeMachineConfiguration(machine.Id, ingredients, recipes);

        return configuration;
    } 

    public async Task<Result> ApproveMachine(ApproveCoffeeMachineForm form)
    {
        if (Uri.TryCreate(form.MachineHealthCheckEndpointUrl, UriKind.Absolute, out _))
            return Result.Failure("Uri is invalid");
        
        var machine = await _dc.CoffeeMachines.ExcludeDeleted().FirstOrDefaultAsync(u => u.Id == form.MachineId);
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
        var machine = await _dc.CoffeeMachines.ExcludeDeleted().FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return Result.Failure("The machine not found");

        machine.State = CoffeeMachineStates.Unavailable;
        
        await _dc.SaveChangesAsync();

        return Result.Success();
    }
    
    public async Task<Result> MakeMachineActive(Guid machineId)
    {
        var machine = await _dc.CoffeeMachines.ExcludeDeleted().FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return Result.Failure("The machine not found");

        machine.State = CoffeeMachineStates.Active;
        
        await _dc.SaveChangesAsync();

        return Result.Success();
    }

    public async Task RunCoffeeMachineObservers()
    {
        var machines = await _dc.CoffeeMachines
            .ActualReadOnly()
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