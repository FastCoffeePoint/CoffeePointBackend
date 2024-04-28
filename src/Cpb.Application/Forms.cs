namespace Cpb.Application;

public record RegisterUserForm(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record LoginUserForm(string Email, string Password);

public record AuthResponse(Guid UserId, string JwtToken);

public record AttachRoleForm(Guid UserId, string Role);
