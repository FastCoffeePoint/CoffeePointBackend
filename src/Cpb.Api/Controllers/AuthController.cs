using Cpb.Api.AspNetCore;
using Cpb.Application;
using Cpb.Application.Services;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cpb.Api.Controllers;

public class AuthController(AuthService authService) : CoffeePointController
{
    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<AuthResponse, string>> Login([FromBody] LoginUserForm form) =>
    await authService.Login(form);

    [HttpPost(DefaultUrl)]
    public async Task<JsonResult<AuthResponse, string>> Register([FromBody] RegisterUserForm form) =>
        await authService.Register(form);

    [Authorize]
    [HttpGet(DefaultUrl)]
    public JsonResult<bool, string> TestAuthentication() =>
        Result.Success<bool, string>(true);
}
