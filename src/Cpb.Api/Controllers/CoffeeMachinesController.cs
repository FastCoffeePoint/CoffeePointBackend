using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

public class CoffeeMachinesController(CoffeeMachinesService _machinesService) : CoffeePointController
{
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> RegisterCoffeeMachine(RegisterCoffeeMachineForm form) => 
        await _machinesService.RegisterCoffeeMachine(form);
    
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<Guid, string>> ApproveMachine(ApproveCoffeeMachineForm form) => 
        await _machinesService.ApproveMachine(form);
}