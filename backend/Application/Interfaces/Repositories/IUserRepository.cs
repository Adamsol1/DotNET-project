using backend.Domain.Models;

namespace backend.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Get user by their username
    /// </summary>
    Task<User?> GetUserByUsername(string username);

    /// <summary>
    /// Get username associated with given user ID
    /// </summary>
    Task<string?> GetUsernameById(int id);


    /// <summary>
    /// Limitied just for authentication testing purpose
    /// Get hashed password associated with given user ID
    /// </summary>
    Task<string?> GetPasswordById(int id);

    /// <summary>
    /// Get the role of the user associated with given user ID
    /// Only one role can be applied to a user
    /// </summary>
    
    Task<string?> GetUserRoleById(int id);
}