﻿namespace Cpb.Database.Entities;

public class DbOrder: DbEntity
{
    public Guid Id { get; set; }

    public Guid CoffeeRecipeId { get; set; }
    public DbCoffeeRecipe CoffeeRecipe { get; set; }

    public Guid UserId { get; set; }
    public DbUser User { get; set; }
}