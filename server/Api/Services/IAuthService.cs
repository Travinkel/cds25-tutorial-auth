using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;

namespace Api.Services;

public interface IAuthService
{
    AuthUserInfo Authenticate(LoginRequest request);
    Task<AuthUserInfo> Register(RegisterRequest request);
}
