using System.Text;
using Cpb.Application.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Cpb.Api.AspNetCore;

public static class StartupExtenstions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, ConfigurationManager configuration)
    {
        var authOptions = configuration.GetSection(nameof(AuthOptions)).Get<AuthOptions>();
        if (authOptions is null)
            throw new ArgumentNullException($"Cannot get {nameof(AuthOptions)}");

        services.AddAuthentication(options =>
         {
             options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

         }).AddJwtBearer(jwtOptions =>
         {
             jwtOptions.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = authOptions.Issuer,
                 ValidAudience = authOptions.Audience,
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.SecretKey))
             };
         });

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.Name));

        return services;
    }
}
