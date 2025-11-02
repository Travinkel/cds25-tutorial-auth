using System;
using System.Linq;
using System.Threading.Tasks;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Api.Etc;

public class DbSeeder
{
    private readonly AppDbContext context;
    private readonly IPasswordHasher<User> hasher;

    public DbSeeder(AppDbContext context, IPasswordHasher<User> hasher)
    {
        this.context = context;
        this.hasher = hasher;
    }

    public async Task SetupAsync(string defaultPassword)
    {
        await context.Database.EnsureCreatedAsync();

        if (!context.Users.Any())
        {
            var users = new (string email, string role)[]
            {
                ("admin@example.com", Role.Admin.ToString()),
                ("editor@example.com", Role.Editor.ToString()),
                ("othereditor@example.com", Role.Editor.ToString()),
                ("reader@example.com", Role.Reader.ToString()),
            };

            await CreateUsers(users, defaultPassword);
        }

        if (!context.Posts.Any(p => p.PublishedAt != null))
        {
            var admin = context.Users.Single(user => user.Email == "admin@example.com");
            context.Posts.Add(new Post
            {
                Title = "First post",
                Content = @"## Hello Python
Have you ever wondered how to make a hello-world application in Python?

The answer is simply:
```py
print('Hello World!')
```",
                AuthorId = admin!.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
            });
        }

        if (!context.Posts.Any(p => p.PublishedAt == null))
        {
            var editor = context.Users.Single(user => user.Email == "editor@example.com");
            context.Posts.Add(new Post
            {
                Title = "Draft",
                Content = "This is a draft post",
                AuthorId = editor!.Id,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = null,
            });
        }

        await context.SaveChangesAsync();

        if (!context.Comments.Any())
        {
            var reader = context.Users.Single(user => user.Email == "reader@example.com");
            context.Comments.Add(new Comment
            {
                Content = "First one to comment",
                AuthorId = reader.Id,
                PostId = context.Posts.First().Id,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task CreateUsers((string email, string role)[] users, string defaultPassword)
    {
        foreach (var u in users)
        {
            var newUser = new User
            {
                UserName = u.email.Split('@')[0],
                Email = u.email,
                EmailConfirmed = true,
                Role = u.role,
            };

            newUser.PasswordHash = hasher.HashPassword(newUser, defaultPassword);

            context.Users.Add(newUser);
        }

        await context.SaveChangesAsync();
    }
}
