using System.Security.Authentication;
using Api.Etc;
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Api.Services;

public class AuthService(
    ILogger<AuthService> _logger,
    IPasswordHasher<User> _passwordHasher,
    IRepository<User> _userRepository
) : IAuthService
{
    public AuthUserInfo Authenticate(LoginRequest request)
    {
        var user = _userRepository.Query().FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
        {
            _logger.LogWarning("Authentication failed: user not found ({Email})", request.Email);
            throw new AuthenticationError();
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result != PasswordVerificationResult.Success)
        {
            _logger.LogWarning("Authentication failed: invalid password for {Email}", request.Email);
            throw new AuthenticationError();
        }

        return new AuthUserInfo(
            Id: user.Id,
            UserName: user.UserName,
            Role: user.Role
        );
    }

    public async Task<AuthUserInfo> Register(RegisterRequest request)
    {
        throw new NotImplementedException();
    }
}
