using backend.Application.Dtos.Authentication;
using backend.Application.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Base;
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
        var appDbContext =
            serviceProvider.GetRequiredService<AppDbContext>();
        var uow =
            serviceProvider.GetRequiredService<IUnitOfWork>();

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
            var authUser = new AuthUser { UserName = "admin" };

            var createAdminUser = await userManager.CreateAsync(
                authUser,
                "Admin123!"
            );

            if (createAdminUser.Succeeded)
            {
                await userManager.AddToRoleAsync(authUser, "admin");
                logger.Information("Created admin user in AuthDb");
                
                // Check if admin user already exists in GameDb
                var existingGameUser = await uow.UserRepository.GetUserByUsername("admin");
                if (existingGameUser == null)
                {
                    // Create user in GameDb with the AuthUserId
                    var gameUser = new User
                    {
                        Username = "admin",
                        AuthUserId = authUser.Id,
                        Role = UserRole.admin
                    };
                    
                    await uow.UserRepository.Create(gameUser);
                    await uow.SaveAsync();
                    logger.Information("Created admin user in GameDb");
                }
                else
                {
                    logger.Information("Admin user already exists in GameDb");
                }
            }
            else
            {
                logger.Error("Failed to create the default admin user. Errors: {Errors}", 
                    string.Join(", ", createAdminUser.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Admin user exists in AuthDb, check if it exists in GameDb
            var existingGameUser = await uow.UserRepository.GetUserByUsername("admin");
            if (existingGameUser == null)
            {
                // Create user in GameDb with the existing AuthUserId
                var gameUser = new User
                {
                    Username = "admin",
                    AuthUserId = checkAdminUser.Id,
                    Role = UserRole.admin
                };
                
                await uow.UserRepository.Create(gameUser);
                await uow.SaveAsync();
                logger.Information("Synced admin user from AuthDb to GameDb");
            }
        }
    }
}