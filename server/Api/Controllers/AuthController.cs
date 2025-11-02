using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using Api.Services;
using Api.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service, ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var userInfo = service.Authenticate(request);
        var token = tokenService.CreateToken(userInfo);
        return new LoginResponse(token);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<RegisterResponse> Register([FromBody] RegisterRequest request)
    {
        var userInfo = await service.Register(request);
        return new RegisterResponse(UserName: userInfo.UserName);
    }

    [HttpGet("userinfo")]
    [Authorize]
    public async Task<AuthUserInfo?> UserInfo()
    {
    if (!User.Identity?.IsAuthenticated ?? true)
        return null;

    return service.GetUserInfo(User);
    }

    [HttpPost("logout")]
    public async Task<IResult> Logout()
    {
        // JWT logout is client-side only (we just forget the token)
        return Results.Ok();
    }
}
