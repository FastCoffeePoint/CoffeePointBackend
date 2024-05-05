namespace Cpb.Database.Entities;

public class DbCoffeeRecipeIngredient
{
    public DbCoffeeRecipe CoffeeRecipe { get; set; }
    public Guid CoffeeRecipeId { get; set; }
    
    public DbIngredient Ingredient { get; set; }
    public Guid IngredientId { get; set; }

    public int Amount { get; set; }
}