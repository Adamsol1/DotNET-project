using backend.Domain.Models;

namespace backend.Infrastructure.Repositories.Base;


public interface IPlayerCharacterRepository : IGenericRepository<PlayerCharacter>
{
    /// <summary>
    /// Get health of a player character by its ID.
    /// </summary>
    Task<int> GetHealthByIdAsync(int id);
    
}