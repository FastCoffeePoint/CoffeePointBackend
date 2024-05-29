namespace Cpb.Database.Entities;

public class DbCoffeeMachineIngredient : DbEntity
{
    public DbCoffeeMachine CoffeeMachine { get; set; }
    public Guid CoffeeMachineId { get; set; }
    
    public DbIngredient Ingredient { get; set; }
    public Guid IngredientId { get; set; }

    /// <summary>
    /// How many ingredients a machine has theoretically.
    /// </summary>
    public int Amount { get; set; }
}