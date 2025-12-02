using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Base;

namespace backend.Infrastructure.Repositories.Implementations;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    /// <summary>
    /// Get user by their username
    /// The method expects either one or zero results because usernames are unique.
    /// </summary>

    public async Task<User?> GetUserByUsername(string username)
    {
        return await GetByProperty(u => u.Username, username);
    }


    /// <summary>
    /// Get username associated with given user ID
    /// The method expects either one or zero results because usernames are unique.
    /// </summary>

    public async Task<string?> GetUsernameById(int id)
    {
        return await GetPropertyValue(id, u => u.Username);
    }


    /// <summary>
    /// Limitied just for authentication testing purpose
    /// Get hashed password associated with given user ID
    /// </summary>

    public async Task<string?> GetPasswordById(int id)
    {
        return await GetPropertyValue(id, u => u.Password);
    }

    /// <summary>
    /// Get the role of the user associated with given user ID
    /// The method expects either one or zero results because only one role is given to each user
    /// </summary>
    
    public async Task<string?> GetUserRoleById(int id)
    {
        var role = await GetPropertyValue(id, u => u.Role);
        return role.ToString(); // Role is an enum
    }

    /// <summary>
    /// Get user by their AuthUserId (Identity user ID)
    /// </summary>
    public async Task<User?> GetByAuthId(string authUserId)
    {
        return await GetByProperty(u => u.AuthUserId, authUserId);
    }

}