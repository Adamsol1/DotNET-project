using backend.Domain.Models;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using backend.Infrastructure.Logging;
using System;
using backend.Infrastructure.Repositories.Base;

namespace backend.Infrastructure.Repositories.Implementations;

/*

 */

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly AppDbContext _db;
    private readonly IEntityFileLogger _entityLogger;

    public UserRepository(AppDbContext db, IEntityFileLogger entityLogger) : base(db, entityLogger)
    {
        _db = db;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Get user by their username
    /// The method expects either one or zero results because usernames are unique.
    /// </summary>

    public async Task<User?> GetUserByUsername(string username)
    {
        try {
            return await GetByProperty(u => u.Username, username);
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetUserByUsernameError", 
                new { 
                    Username = username,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.System);
            throw;
        }
    }


    /// <summary>
    /// Get username associated with given user ID
    /// The method expects either one or zero results because usernames are unique.
    /// </summary>

    public async Task<string?> GetUsernameById(int id)
    {
        try {
            return await GetPropertyValue(id, u => u.Username);
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetUsernameByIdError", 
                new { 
                    Id = id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.System);
            throw;
        }
    }


    /// <summary>
    /// Limitied just for authentication testing purpose
    /// Get hashed password associated with given user ID
    /// </summary>

    /// <summary>
    /// Get the role of the user associated with given user ID
    /// The method expects either one or zero results because only one role is given to each user
    /// </summary>
    
    public async Task<string?> GetUserRoleById(int id)
    {
        try {
        var role = await GetPropertyValue(id, u => u.Role);
        return role.ToString(); // Role is an enum

        } catch (Exception ex) {

            await _entityLogger.LogAsync(
                "GetUserRoleByIdError", 
                new { 
                    Id = id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.System);
            throw;
        }
    }

    /// <summary>
    /// Get user by their AuthUserId (Identity user ID)
    /// </summary>
    public async Task<User?> GetByAuthId(string authUserId)
    {
        try {
            return await GetByProperty(u => u.AuthUserId, authUserId);
        
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetByAuthIdError", 
                new { 
                    AuthUserId = authUserId,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.System);
            throw;
        }
    }

}