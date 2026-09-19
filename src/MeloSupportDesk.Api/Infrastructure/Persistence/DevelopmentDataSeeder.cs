using MeloSupportDesk.Api.Domain.Entities;
using MeloSupportDesk.Api.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeloSupportDesk.Api.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        string password)
    {
        using var scope = services.CreateScope();

        var database = scope.ServiceProvider
            .GetRequiredService<SupportDeskDbContext>();

        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<IPasswordHasher<User>>();

        var demoUsers = new[]
        {
            new User
            {
                FullName = "Demo Administrator",
                Email = "admin@melosupportdesk.local",
                Role = UserRole.Admin,
                IsActive = true
            },
            new User
            {
                FullName = "Demo Technician",
                Email = "technician@melosupportdesk.local",
                Role = UserRole.Technician,
                IsActive = true
            }
        };

        foreach (var user in demoUsers)
        {
            var userExists = await database.Users
                .AnyAsync(existingUser =>
                    existingUser.Email == user.Email);

            if (userExists)
            {
                continue;
            }

            user.PasswordHash = passwordHasher.HashPassword(
                user,
                password);

            database.Users.Add(user);
        }

        await database.SaveChangesAsync();
    }
}
