using System.Collections.Immutable;

namespace Cpb.Domain;


// CoffeeRecipe
public record CoffeeRecipe(Guid Id, string Name, ImmutableList<CoffeeRecipeIngredient> Ingredients);

public record CustomerCoffeeRecipe(Guid Id, string Name, int AvailableAmount , ImmutableList<CoffeeRecipeIngredient> Ingredients);

public record CoffeeRecipeIngredient(Guid Id, string Name, int Amount);

public record Ingredient(Guid Id, string Name);

// Coffee machine
public record CoffeeMachine(Guid Id, string Name, ImmutableList<CoffeeMachineIngredient> Ingredients);
public record CoffeeMachineIngredient(Guid Id, string Name, int Amount);