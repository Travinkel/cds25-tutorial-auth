using System;
using System.Security.Claims;
using Api.Models.Dtos.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

public interface ITokenService
{
    string CreateToken(AuthUserInfo user);
}

public class JwtService : ITokenService
{
    private readonly IConfiguration _config;
    public const string SignatureAlgorithm = SecurityAlgorithms.HmacSha512;
    public const string JwtKey = "JwtKey";

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(AuthUserInfo user)
    {
        var keyBase64 = _config.GetValue<string>(JwtKey)
                         ?? throw new InvalidOperationException("JwtKey not configured");
        var key = Convert.FromBase64String(keyBase64);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SignatureAlgorithm
            ),
            Subject = new ClaimsIdentity(user.ToClaims()),
            Expires = DateTime.UtcNow.AddDays(7),
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    public static TokenValidationParameters ValidationParameters(IConfiguration config)
    {
        var keyBase64 = config.GetValue<string>(JwtKey)
                         ?? throw new InvalidOperationException("JwtKey not configured");
        var key = Convert.FromBase64String(keyBase64);
        return new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidAlgorithms = new[] { SignatureAlgorithm },
            ValidateIssuerSigningKey = true,

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero,
        };
    }
}