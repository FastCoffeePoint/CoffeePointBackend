using Cpb.Domain;

namespace Cpb.Database.Entities;

public class DbOrder: DbEntity
{
    public Guid Id { get; set; }

    public OrderStates State { get; set; }
    
    public Guid CoffeeRecipeId { get; set; }
    public DbCoffeeRecipe CoffeeRecipe { get; set; }

    public Guid UserId { get; set; }
    public DbUser User { get; set; }

    public List<DbLockedIngredient> LockedIngredients { get; set; }
}