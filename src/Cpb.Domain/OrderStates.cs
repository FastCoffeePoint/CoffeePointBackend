namespace Cpb.Domain;

public enum OrderStates
{
    /// <summary>
    /// A coffee is waiting to start brewing in coffee machine.
    /// </summary>
    InOrder = 0,
    /// <summary>
    /// A coffee is brewing.
    /// </summary>
    IsBrewing = 1,
    /// <summary>
    /// A coffee is ready to be gotten by a customer.
    /// </summary>
    IsReadyToBeGotten = 2,
    /// <summary>
    /// A coffee has gotten by a customer.
    /// </summary>
    Complete = 3,
    /// <summary>
    /// The process of executing an order has failed at some stage. 
    /// </summary>
    Failed = 4,
}