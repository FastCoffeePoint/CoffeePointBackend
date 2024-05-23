using System.Collections.Immutable;

namespace Cpb.Domain;

// Ingredients
public record Ingredient(Guid Id, string Name);

// CoffeeRecipe
public record CoffeeRecipe(Guid Id, string Name, ImmutableList<CoffeeRecipeIngredient> Ingredients);

public record CustomerCoffeeRecipe(Guid Id, string Name, int AvailableAmount , ImmutableList<CoffeeRecipeIngredient> Ingredients);

public record CoffeeRecipeIngredient(Guid Id, string Name, int Amount);


// Coffee machine
public record CoffeeMachine(Guid Id, string Name, ImmutableList<CoffeeMachineIngredient> Ingredients);
public record CoffeeMachineIngredient(Guid Id, string Name, int Amount);


// Orders
public record LockedRecipe(Guid OrderId, Guid RecipeId);