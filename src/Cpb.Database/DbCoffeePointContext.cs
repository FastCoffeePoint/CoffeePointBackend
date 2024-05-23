using Cpb.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cpb.Database;

public class DbCoffeePointContext : DbContext
{
    public DbCoffeePointContext(DbContextOptions<DbCoffeePointContext> options) : base(options)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<DbUser> Users { get; set; }
    
    public DbSet<DbIngredient> Ingredients { get; set; }
    
    public DbSet<DbCoffeeMachine> CoffeeMachines { get; set; }
    public DbSet<DbCoffeeMachineIngredient> CoffeeMachineIngredients { get; set; }
    
    public DbSet<DbCoffeeRecipe> CoffeeRecipes  { get; set; }
    public DbSet<DbCoffeeRecipeIngredient> CoffeeRecipeIngredients  { get; set; }

    public DbSet<DbOrder> Orders { get; set; }
    public DbSet<DbLockedIngredient> LockedIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbCoffeeRecipe>()
            .HasMany(u => u.Ingredients)
            .WithMany(u => u.CoffeeRecipes)
            .UsingEntity<DbCoffeeRecipeIngredient>();
        
        modelBuilder.Entity<DbCoffeeMachine>()
            .HasMany(u => u.Ingredients)
            .WithMany(u => u.CoffeeMachines)
            .UsingEntity<DbCoffeeMachineIngredient>();
    }
}