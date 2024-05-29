using System.IdentityModel.Tokens.Jwt;
using Cpb.Domain;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
        // User.Claims has wrong claim types and I don't know why, probably because of the configuration. 
        var wasHeaderFound = Request.Headers.TryGetValue(HeaderNames.Authorization, out var token);
        if (!wasHeaderFound) 
            throw new Exception($"Can't find JWT in request header");
        token = token.ToString().Replace("Bearer ", "");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var stringGuid = jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub);
        if (stringGuid == null)
            throw new Exception($"Can't find claim with user id");

        var wasParsed = Guid.TryParse(stringGuid.Value, out var userId);
        if(!wasParsed)
            throw new Exception($"Can't find a claim with the user id {stringGuid.Value}");

        _actor = new Actor(userId);

        return _actor;
    }

    protected static JsonResult<TResult, string> JsonResult<TResult>(Maybe<TResult> r) => r.HasValue
        ? new() { Error = default, Result = r.Value, IsSuccess = true }
        : new() { Error = $"A {nameof(TResult)} was not found", Result = default, IsSuccess = false };
}
