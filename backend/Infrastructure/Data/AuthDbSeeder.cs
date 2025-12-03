using backend.Application.Dtos.Authentication;

using backend.Application.Interfaces;
using backend.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace backend.Infrastructure.Data;

public static class AuthDbSeeder
{
    /// <summary>
    /// Seed the authentication database with roles and default admin user.
    /// The method also applies any pending migrations to the database.
    /// </summary>
    public static async Task SeedAsync(
        AuthDbContext context,
        IServiceProvider serviceProvider,
        Serilog.ILogger logger)
    {
        // Apply migrations
        await context.Database.MigrateAsync();

        // Resolve required services
        var roleManager =
            serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager =
            serviceProvider.GetRequiredService<UserManager<AuthUser>>();
        var userService =
            serviceProvider.GetRequiredService<IUserService>();

        // Seed roles
        var roles = new[] { "player", "admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.Information("Created role: {Role}", role);
            }
        }

        // Seed admin user
        var checkAdminUser = await userManager.FindByNameAsync("admin");
        if (checkAdminUser == null)
        {
            var user = new AuthUser { UserName = "admin2" };

            var createAdminUser = await userManager.CreateAsync(
                user,
                "Admin123!"
            );

            if (createAdminUser.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "admin");
                await userService.RegisterAccount(
                    new RegisterUserDto
                    {
                        Username = "admin2",
                        Password = "Admin123!"
                    }
                );
                logger.Information("Created the default admin user");
            }
            else
            {
                logger.Error("Failed to create the default admin user");
            }
        }
    }
}