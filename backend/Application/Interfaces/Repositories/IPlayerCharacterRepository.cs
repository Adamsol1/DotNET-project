using backend.Domain.Models;

namespace backend.Application.Interfaces.Repositories;


public interface IPlayerCharacterRepository : IGenericRepository<PlayerCharacter>
{
    /// <summary>
    /// Get health of a player character by its ID.
    /// </summary>
    Task<int> GetHealthByIdAsync(int id);
    
}