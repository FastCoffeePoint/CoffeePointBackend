namespace Cpb.Application;

// Auth
public record RegisterUserForm(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record LoginUserForm(string Email, string Password);
public record AuthResponse(Guid UserId, string JwtToken);




// Ingredient
public record CreateIngredientForm(string Name);
public record ReplenishIngredientForm(Guid IngredientId, int IncreaseAmount);


// Coffee recipe
public record CreateCoffeeRecipe(string Name);
public record ManageIngredientInRecipeForm(Guid RecipeId, Guid IngredientId);
