using Cpb.Common;

namespace Cpb.Application.Configurations;

public class AuthOptions : IOptions
{
    public static string Name => "AuthOptions";
    
    public string SecretKey { get; init; }
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public TimeSpan TokenLifetime { get; init; }
}
