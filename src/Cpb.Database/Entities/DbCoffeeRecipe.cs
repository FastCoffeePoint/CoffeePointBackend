using System.ComponentModel.DataAnnotations;

namespace Cpb.Database.Entities;

public class DbCoffeeRecipe : DbEntity
{
    [Key]
    public Guid Id { get; set; }
    
    [MaxLength(128)]
    public string Name { get; set; }

    public List<DbIngredient> Ingredients { get; set; }
    
    public List<DbCoffeeRecipeIngredient> Links { get; set; }

    //public int CurrentOrdersCount { get; set; }
}