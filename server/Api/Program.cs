using Api.Etc;
using Api.Etc.NSwag;
using Api.Services;
using DataAccess;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using NSwag.AspNetCore;
using Microsoft.AspNetCore.Authorization;

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();

        if (args is [.., "setup", var defaultPassword])
        {
            SetupDatabase(app, defaultPassword);
            Environment.Exit(0);
        }

        ConfigureApp(app);

        await app.RunAsync();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Entity Framework
        var connectionString = builder.Configuration.GetConnectionString("AppDb");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );
        builder.Services.AddScoped<DbSeeder>();

        // Security - JWT
        builder.Services.AddScoped<ITokenService, JwtService>();

        // Repositories
        builder.Services.AddScoped<IRepository<User>, UserRepository>();
        builder.Services.AddScoped<IRepository<Post>, PostRepository>();
        builder.Services.AddScoped<IRepository<Comment>, CommentRepository>();

        // Services
        builder.Services.AddScoped<IBlogService, BlogService>();
        builder.Services.AddScoped<IDraftService, DraftService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Security
        builder.Services.AddScoped<IPasswordHasher<User>, BcryptPasswordHasher>();

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // builder.Services.AddOpenApi();
        builder.Services.AddOpenApiDocument(conf =>
        {
            conf.DocumentProcessors.Add(new TypeMapDocumentProcessor<ProblemDetails>());
            conf.SchemaSettings.AlwaysAllowAdditionalObjectProperties = false;
            conf.SchemaSettings.GenerateAbstractProperties = true;
            conf.SchemaSettings.SchemaProcessors.Add(new RequiredSchemaProcessor());
        });

        // Exception handling
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = JwtService.ValidationParameters(builder.Configuration);
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine($"Auth failed: {ctx.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });

        builder.Services.AddAuthorization(options =>
    {
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    });


    }



    public static void SetupDatabase(WebApplication app, string defaultPassword)
    {
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            seeder.SetupAsync(defaultPassword).Wait();
        }
    }

    public static WebApplication ConfigureApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        // Enable OpenAPI / Swagger for development
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Exception handling
    app.UseExceptionHandler();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Map API Controllers
    app.MapControllers();

    // Generate client + expose Scalar UI
    app.GenerateApiClientsFromOpenApi("/../../client/src/models/generated-client.ts").Wait();
    app.MapScalarApiReference();

    return app;
    }
}
