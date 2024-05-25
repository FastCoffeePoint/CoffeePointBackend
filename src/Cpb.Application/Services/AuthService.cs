using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cpb.Application.Configurations;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cpb.Application.Services;

public class AuthService(
    DbCoffeePointContext dc,
    PasswordHasher passwordHasher,
    IOptionsMonitor<AuthOptions> options,
    IOptionsMonitor<DefaultAdminOptions> adminOptions)
{
    public async Task CreateDefaultAdmin()
    {
        var credentials = adminOptions.CurrentValue;
        var admin = await dc.Users.FirstOrDefaultAsync(u => credentials.Email == u.Email);
        
        if (admin != null)
            return;
        
        dc.Users.Add(new DbUser
        {
            Email = credentials.Email,
            FirstName = "Admin",
            LastName = "Adminov",
            HashedPassword = passwordHasher.HashPassword(credentials.Password),
            Role = Roles.Admin
        });

        await dc.SaveChangesAsync();
    }

    public async Task<Result<AuthResponse, string>> Register(RegisterUserForm form)
    {
        var emailIsBusy = await dc.Users.AnyAsync(u => u.Email == form.Email);
        if(emailIsBusy)
            return "The email is busy";

        var hashedPassword = passwordHasher.HashPassword(form.Password);
        var user = dc.Users.Add(new DbUser
        {
            Email = form.Email,
            FirstName = form.FirstName,
            LastName = form.LastName,
            HashedPassword = hashedPassword,
            Role = Roles.Customer
        }).Entity;
        await dc.SaveChangesAsync();

        var jwtToken = GenerateToken(user);
        
        return new AuthResponse(user.Id, jwtToken);
    }

    public async Task<Result<AuthResponse, string>> Login(LoginUserForm form)
    {
        var user = await dc.Users
            .FirstOrDefaultAsync(u => u.Email == form.Email);
        if (user == null)
            return "Wrong password or email";

        if (!passwordHasher.VerifyHashedPassword(user.HashedPassword, form.Password))
            return "Wrong password or email";

        var jwtToken = GenerateToken(user);
        return new AuthResponse(user.Id, jwtToken);
    }

    private string GenerateToken(DbUser user)
    {
        var authOptions = options.CurrentValue;
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Email, user.Email),
            new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(authOptions.Issuer,
            authOptions.Audience,
            claims,
            expires: DateTime.Now.Add(authOptions.TokenLifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
