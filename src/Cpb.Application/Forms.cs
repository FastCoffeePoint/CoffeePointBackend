using Cpb.Common.Kafka;

namespace Cpb.Application;

// Auth
public record RegisterUserForm(string FirstName, string LastName, string Email, string Password);
public record LoginUserForm(string Email, string Password);
public record AuthResponse(Guid UserId, string JwtToken);


// Ingredient
public record CreateIngredientForm(string Name);


// Coffee recipe
public record CreateCoffeeRecipe(string Name);
public record SetIngredientInRecipeForm(Guid RecipeId, Guid IngredientId, int Amount);
public record RemoveIngredientFromRecipeForm(Guid RecipeId, Guid IngredientId);

// Coffee machine
public record RegisterCoffeeMachineForm(string Name);
public record ApproveCoffeeMachineForm(Guid MachineId, string MachineHealthCheckEndpointUrl);
public record SetIngredientInMachineForm(Guid MachineId, Guid IngredientId, int Amount);
public record RemoveIngredientFromMachineForm(Guid MachineId, Guid IngredientId);

// Orders
public record OrderCoffeeForm(Guid RecipeId);

public record CoffeeStartedBrewingEvent(Guid OrderId): IEvent
{
    public static string Name => "CoffeeStartedBrewingEvent";
}
