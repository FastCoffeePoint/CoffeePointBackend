using System.Collections.Immutable;
using Cpb.Common;

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
public record CoffeeMachineConfiguration(Guid MachineId, ImmutableList<ConfigurationIngredient> Ingredients, ImmutableList<ConfigurationRecipe> Recipes);
public record ConfigurationIngredient(Guid IngredientId, string SensorId);
public record ConfigurationRecipe(Guid RecipeId, string SensorId);


// Orders
public record LockedRecipe(Guid OrderId, Guid RecipeId);
public record Order(Guid Id, OrderStates State, Guid RecipeId, Guid UserId);