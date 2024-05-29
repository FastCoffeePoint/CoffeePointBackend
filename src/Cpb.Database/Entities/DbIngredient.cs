using System.ComponentModel.DataAnnotations;

namespace Cpb.Database.Entities;

public class DbIngredient : DbEntity
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(128)]
    public string Name { get; set; }
    
    public List<DbCoffeeRecipe> CoffeeRecipes { get; set; }
    public List<DbCoffeeRecipeIngredient> Links { get; set; }

    public List<DbCoffeeMachine> CoffeeMachines { get; set; }
}