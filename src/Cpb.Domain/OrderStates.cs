using System.Text.Json.Serialization;

namespace Cpb.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStates
{
    /// <summary>
    /// A coffee is waiting to start brewing in coffee machine.
    /// </summary>
    InQueue = 0,
    /// <summary>
    /// A coffee is brewing.
    /// </summary>
    Brewing = 1,
    /// <summary>
    /// A coffee is ready to be gotten by a customer.
    /// </summary>
    ReadyToBeGotten = 2,
    /// <summary>
    /// A coffee has gotten by a customer.
    /// </summary>
    Complete = 3,
    /// <summary>
    /// The process of executing an order has failed at some stage. 
    /// </summary>
    Failed = 4,
}