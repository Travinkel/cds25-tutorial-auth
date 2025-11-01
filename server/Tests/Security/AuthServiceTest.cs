using System.Security.Authentication;
using Api.Etc;
using Api.Models.Dtos.Requests;
using Api.Services;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Tests.Helpers;
using TUnit.Assertions;


namespace Tests.Security;

public class AuthServiceTest
{
    IRepository<User> userRepository = null!;
    IPasswordHasher<User> passwordHasher = null!;
    IAuthService sut = null!;

    [Before(Test)]
    public void Setup()
    {
        passwordHasher = new FakePasswordHasher<User>();
        String Hash(string password) => passwordHasher.HashPassword(null!, password);
        userRepository = new InMemoryRespository<User>(
            new List<User>
            {
                new User()
                {
                    Id = "user1",
                    UserName = "User1",
                    Email = "user1@example.com",
                    PasswordHash = Hash("fakepassword"),
                    Role = Role.Reader,
                },
                new User()
                {
                    Id = "user2",
                    UserName = "User2",
                    Email = "user2@example.com",
                    PasswordHash = Hash("fakepassword"),
                    Role = Role.Admin,
                },
            }
        );
        sut = new AuthService(
            new LoggerFactory().CreateLogger<AuthService>(),
            passwordHasher,
            userRepository
        );
    }

    [Test]
    public async Task Authenticate_Success()
    {
        var response = sut.Authenticate(new LoginRequest("user1@example.com", "fakepassword"));
        await Assert.That(response.UserName).IsEqualTo("User1");
    }

    [Test]
    public async Task Authenticate_InvalidEmail()
    {
        await Assert
            .That(() => sut.Authenticate(new LoginRequest("invalid", "fakepassword")))
            .Throws<AuthenticationError>();
    }

    [Test]
    public async Task Authenticate_InvalidPassword()
    {
        await Assert
            .That(() => sut.Authenticate(new LoginRequest("user1@example.com", "invalid")))
            .Throws<AuthenticationError>();
    }
}