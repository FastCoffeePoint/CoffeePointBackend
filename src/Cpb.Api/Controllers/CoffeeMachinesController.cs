using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

public class CoffeeMachinesController(CoffeeMachinesService _machinesService) : CoffeePointController
{
    [HttpPut(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> SetIngredientInMachine(SetIngredientInMachineForm form) => 
        await _machinesService.SetIngredientInMachine(form);
    
    [HttpDelete(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> RemoveIngredientFromMachine(RemoveIngredientFromMachineForm form) => 
        await _machinesService.RemoveIngredientFromMachine(form);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> RegisterCoffeeMachine(RegisterCoffeeMachineForm form) => 
        await _machinesService.RegisterCoffeeMachine(form);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> ApproveMachine(ApproveCoffeeMachineForm form) => 
        await _machinesService.ApproveMachine(form);
}