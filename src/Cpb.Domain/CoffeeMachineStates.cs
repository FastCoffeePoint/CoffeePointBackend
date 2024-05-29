using System.Text.Json.Serialization;

namespace Cpb.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CoffeeMachineStates
{
    Unavailable = 0,
    Active = 1,
    Disabled = 2,
    WaitingApprove
}