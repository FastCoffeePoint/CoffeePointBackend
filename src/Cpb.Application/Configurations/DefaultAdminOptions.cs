using Cpb.Common;

namespace Cpb.Application.Configurations;
public class DefaultAdminOptions : IOptions
{
    public static string Name => "DefaultAdminOptions";

    public string Email { get; init; }
    public string Password { get; init; }
}
