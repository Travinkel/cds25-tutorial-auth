using Api;
using DataAccess.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TUnit;
using TUnit.Assertions;

# pragma warning disable CS8625, CS8618

namespace Tests.Security;

public class PasswordHasherTest
{
    IPasswordHasher<User> sut;

    [Before(Test)]
    public void Setup()
    {
        var builder = WebApplication.CreateBuilder();
        Program.ConfigureServices(builder);
        var app = builder.Build();
        sut = app.Services.GetRequiredService<IPasswordHasher<User>>();
        Console.WriteLine($"Using password hasher: {sut.GetType().Name}");
    }

    [Test]
    public async Task HashAndVerifyPassword()
    {
        var password = "S3cret!1";
        var hash = sut.HashPassword(null, password);
        var result = sut.VerifyHashedPassword(null, hash, password);
        await Assert.That(result).IsEqualTo(PasswordVerificationResult.Success);
    }
}
