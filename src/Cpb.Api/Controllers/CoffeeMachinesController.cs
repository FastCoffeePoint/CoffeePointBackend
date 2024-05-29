using System.Collections.Immutable;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

[Roles(Roles.Admin)]
public class CoffeeMachinesController(CoffeeMachinesService _machinesService) : CoffeePointController
{
    [HttpGet(DefaultUrl)]
    public async Task<ImmutableList<CoffeeMachine>> GetCoffeeMachines() => 
        await _machinesService.GetCoffeeMachines();
    
    [HttpPut(DefaultUrl)]
    public async Task<JsonOptionError> SetIngredientInMachine(SetIngredientInMachineForm form) => 
        await _machinesService.SetIngredientInMachine(form);
    
    [HttpDelete(DefaultUrl)]
    public async Task<JsonOptionError> RemoveIngredientFromMachine(RemoveIngredientFromMachineForm form) => 
        await _machinesService.RemoveIngredientFromMachine(form);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> RegisterCoffeeMachine(RegisterCoffeeMachineForm form) => 
        await _machinesService.RegisterCoffeeMachine(form);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonOptionError> ApproveMachine(ApproveCoffeeMachineForm form) => 
        await _machinesService.ApproveMachine(form);

    [HttpGet(DefaultUrl)]
    public async Task<IActionResult> GetMachineConfiguration([FromQuery] Guid machineId)
    {
        var configuration = await _machinesService.GetConfiguration(machineId);
        if (configuration.IsFailure)
            return Ok(Result.Failure<FileContentResult, string>(configuration.Error));
        
        var json = JsonSerializer.Serialize(configuration.Value, new JsonSerializerOptions { WriteIndented = true });
        var jsonBytes = Encoding.UTF8.GetBytes(json);

        return File(jsonBytes, MediaTypeNames.Application.Json, "appsettings.json");
    }
        
}