using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Cpb.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

[Roles(Roles.Admin)]
public class CoffeeMachinesController(CoffeeMachinesService _machinesService) : CoffeePointController
{
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
}