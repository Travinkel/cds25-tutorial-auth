using System.Security.Authentication;
using Api.Etc;
using Api.Models.Dtos.Requests;
using Api.Models.Dtos.Responses;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Api.Security;
using Api.Mappers;

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

    public AuthUserInfo? GetUserInfo(ClaimsPrincipal principal)
{
    var userId = principal.GetUserId();
    return _userRepository
        .Query()
        .Where(u => u.Id == userId)
        .SingleOrDefault()
        ?.ToDto();
}

    public async Task<AuthUserInfo> Register(RegisterRequest request)
    {
    // 1. check if email already exists
    var existingUser = _userRepository
        .Query()
        .FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

    if (existingUser != null)
        throw new ValidationException("Email is already in use.");

    // 2. map RegisterRequest → User entity
    var newUser = new User
{
    Id = Guid.NewGuid().ToString(),
    Email = request.Email,
    UserName = request.UserName,
    Role = Role.Reader
};

    // 3. hash the password
    newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);

    // 4. save user
    await _userRepository.Add(newUser);

    // 5. map to DTO and return
    return new AuthUserInfo(
        Id: newUser.Id,
        UserName: newUser.UserName,
        Role: newUser.Role
    );
    }
}