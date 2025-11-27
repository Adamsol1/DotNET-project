using backend.Application.Dtos;

// Player Service Interface

namespace backend.Application.Interfaces.Services;

public interface IPlayerService
{
   

    // delete player character
    Task<bool> DeleteCharacter(int id);
    
    // ===== HEALTH MANAGEMENT METHODS =====
    
    /// <summary>
    /// Modify player's health by a specific amount (positive or negative)
    /// </summary>
    Task<int> ModifyHealth(int playerCharacterId, int healthChange);
    
  
    
    /// <summary>
    /// Get player's current health
    /// </summary>
    Task<int> GetHealth(int playerCharacterId);
    
    /// <summary>
    /// Check if player is alive (health > 0)
    /// </summary>
    Task<bool> IsAlive(int playerCharacterId);
    
}