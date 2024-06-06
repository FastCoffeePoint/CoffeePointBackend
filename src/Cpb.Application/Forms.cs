using System.Collections.Immutable;
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
public record CoffeeMachineIngredientForm(Guid Id, int AmountBeforeExecution, int AmountAfterExecution);
public record CoffeeStartedBrewingEvent(Guid OrderId, Guid MachineId): IEvent
{
    public static string Name => "CoffeeStartedBrewingEvent";
}
public record CoffeeWasOrderedEvent(Guid OrderId, Guid RecipeId): IEvent
{
    public static string Name => "CoffeeWasOrderedEvent";
}
public record CoffeeIsReadyToBeGottenEvent(Guid MachineId, Guid OrderId, ImmutableList<CoffeeMachineIngredientForm> Ingredients): IEvent
{
    public static string Name => "CoffeeIsReadyToBeGottenEvent";
}
public record OrderHasBeenCompletedEvent(Guid OrderId): IEvent
{
    public static string Name => "OrderHasBeenCompletedEvent";
}
public record OrderHasBeenFailedEvent(Guid OrderId, Guid ErrorCode): IEvent
{
    public static string Name => "OrderHasBeenFailedEvent";
}


