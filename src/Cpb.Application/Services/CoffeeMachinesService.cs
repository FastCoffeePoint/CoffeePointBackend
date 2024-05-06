using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Application.Services;

public class CoffeeMachinesService(DbCoffeePointContext _dc, IHttpClientFactory _httpFactory)
{
    public async Task<Result<Guid, string>> RegisterCoffeeMachine(RegisterCoffeeMachineForm form)
    {
        if (string.IsNullOrEmpty(form.Name) || form.Name.Length > DbCoffeeMachine.NameMaxLength)
            return "Name of the coffee machine is invalid";

        var machine = new DbCoffeeMachine
        {
            Name = form.Name,
            State = CoffeeMachineState.WaitingApprove,
            MachineHealthCheckEndpointUrl = null,
        }.MarkCreated();
        _dc.CoffeeMachines.Add(machine);
        await _dc.SaveChangesAsync();

        return machine.Id;
    }

    public async Task<Result<Guid, string>> ApproveMachine(ApproveCoffeeMachineForm form)
    {
        if (Uri.TryCreate(form.MachineHealthCheckEndpointUrl, UriKind.Absolute, out _))
            return "Uri is invalid";
        
        var machine = await _dc.CoffeeMachines.FirstOrDefaultAsync(u => u.Id == form.MachineId);
        if (machine == null)
            return "The machine not found";

        if (machine.State != CoffeeMachineState.WaitingApprove)
            return machine.Id;
        
        machine.MachineHealthCheckEndpointUrl = form.MachineHealthCheckEndpointUrl;
        machine.State = CoffeeMachineState.Active;

        await _dc.SaveChangesAsync();

        return machine.Id;
    }

    public async Task<Result<Guid, string>> MakeMachineUnavailable(Guid machineId)
    {
        var machine = await _dc.CoffeeMachines.FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return "The machine not found";

        machine.State = CoffeeMachineState.Unavailable;
        
        await _dc.SaveChangesAsync();

        return machine.Id;
    }
    
    public async Task<Result<Guid, string>> MakeMachineActive(Guid machineId)
    {
        var machine = await _dc.CoffeeMachines.FirstOrDefaultAsync(u => u.Id == machineId);
        if (machine == null)
            return "The machine not found";

        machine.State = CoffeeMachineState.Active;
        
        await _dc.SaveChangesAsync();

        return machine.Id;
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
        }
    }
}