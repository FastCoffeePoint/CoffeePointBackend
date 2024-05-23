namespace Cpb.Database.Entities;

public class DbCoffeeRecipeIngredient
{
    public DbCoffeeRecipe CoffeeRecipe { get; set; }
    public Guid CoffeeRecipeId { get; set; }
    
    public DbIngredient Ingredient { get; set; }
    public Guid IngredientId { get; set; }

    /// <summary>
    /// How many ingredients we need to make a recipe.
    /// </summary>
    public int Amount { get; set; }
}