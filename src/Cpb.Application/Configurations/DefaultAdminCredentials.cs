using Cpb.Common;

namespace Cpb.Application.Configurations;
public class DefaultAdminCredentials : IOptions
{
    public static string Name => "DefaultAdminCredentials";

    public string Email { get; init; }
    public string Password { get; init; }
}
