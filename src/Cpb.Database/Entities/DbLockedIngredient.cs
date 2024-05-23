namespace Cpb.Database.Entities;

public class DbLockedIngredient: DbEntity
{
    public Guid OrderId { get; set; }
    public DbOrder Order { get; set; }

    public Guid IngredientId { get; set; }
    public DbIngredient Ingredient { get; set; }

    public Guid RecipeId { get; set; }
    public DbCoffeeRecipe Recipe { get; set; }

    public int LockedAmount { get; set; }
}