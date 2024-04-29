using System.IdentityModel.Tokens.Jwt;
using Cpb.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.AspNetCore;

public class CoffeePointController : ControllerBase
{
    protected const string DefaultUrl = "[controller]/[action]";
    private Actor _actor;

    /// <summary>
    /// Current authorized user
    /// </summary>
    protected Actor Actor => _actor ?? ForciblyGetActor();

    private Actor ForciblyGetActor()
    {
        var stringGuid = User.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub);
        if (stringGuid == null)
            throw new Exception($"Can't find claim with user id");

        var wasParsed = Guid.TryParse(stringGuid.Value, out var userId);
        if(!wasParsed)
            throw new Exception($"Can't find claim with user id");

        _actor = new Actor(userId);

        return _actor;
    }
}
