using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Cpb.Application.Configurations;
using Cpb.Database;
using Cpb.Database.Entities;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cpb.Application.Services;

public class AuthService
{
    private readonly DbCoffeePointContext _dc;
    private readonly PasswordHasher _passwordHasher;
    private readonly IOptionsMonitor<AuthOptions> _authOptions;
    private readonly IOptionsMonitor<DefaultAdminCredentials> _adminOptions;

    public AuthService(DbCoffeePointContext dc,
        PasswordHasher passwordHasher,
        IOptionsMonitor<AuthOptions> authOptions,
        IOptionsMonitor<DefaultAdminCredentials> adminOptions)
    {
        _dc = dc;
        _passwordHasher = passwordHasher;
        _authOptions = authOptions;
        _adminOptions = adminOptions;
    }

    public async Task CreateDefaultAdmin()
    {
        var credentials = _adminOptions.CurrentValue;
        var admin = await _dc.Users.FirstOrDefaultAsync(u => credentials.Email == u.Email);
        
        if (admin != null)
            return;
        
        _dc.Users.Add(new DbUser
        {
            Email = credentials.Email,
            FirstName = "Admin",
            LastName = "Adminov",
            HashedPassword = _passwordHasher.HashPassword(credentials.Password),
            Role = Roles.Admin
        });

        await _dc.SaveChangesAsync();
    }

    public async Task<Result<AuthResponse, string>> Register(RegisterUserForm form)
    {
        var emailIsBusy = await _dc.Users.AnyAsync(u => u.Email == form.Email);
        if(emailIsBusy)
            return "The email is busy";

        var hashedPassword = _passwordHasher.HashPassword(form.Password);
        var user = _dc.Users.Add(new DbUser()
        {
            Email = form.Email,
            FirstName = form.FirstName,
            LastName = form.LastName,
            HashedPassword = hashedPassword,
            Role = Roles.Customer
        }).Entity;
        await _dc.SaveChangesAsync();

        var jwtToken = GenerateToken(user);
        
        return new AuthResponse(user.Id, jwtToken);
    }

    public async Task<Result<AuthResponse, string>> Login(LoginUserForm form)
    {
        var user = await _dc.Users
            .FirstOrDefaultAsync(u => u.Email == form.Email);
        if (user == null)
            return "Wrong password or email";

        if (!_passwordHasher.VerifyHashedPassword(user.HashedPassword, form.Password))
            return "Wrong password or email";

        var jwtToken = GenerateToken(user);
        return new AuthResponse(user.Id, jwtToken);
    }

    private string GenerateToken(DbUser user)
    {
        var authOptions = _authOptions.CurrentValue;
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
