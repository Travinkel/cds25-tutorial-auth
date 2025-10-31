using Api.Etc;
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;

using Api.Services;

public class AuthService(
    ILogger<AuthService> _logger,
    IPasswordHasher<User> _passwordHasher,
    IRepository<User> _userRepository
) : IAuthService

{
    public AuthUserInfo Authenticate(AuthUserRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthUserInfo> Register(RegisterRequest request)
    {
        throw new NotImplementedException();
    }
}