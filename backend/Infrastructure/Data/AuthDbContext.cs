using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using backend.Domain.Models;

/// <summary>
///  AuthDbContext is responsible for managing the authentication-related database operations.
///  It extends IdentityDbContext to leverage ASP.NET Core Identity features.
/// </summary>
public class AuthDbContext : IdentityDbContext<AuthUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
        
    }
}
