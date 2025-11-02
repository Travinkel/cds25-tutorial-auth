using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using System.Security.Claims;

namespace Api.Services;

public interface IAuthService
{
    AuthUserInfo Authenticate(LoginRequest request);
    Task<AuthUserInfo> Register(RegisterRequest request);

    AuthUserInfo? GetUserInfo(ClaimsPrincipal principal);

}
