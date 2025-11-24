using backend.Application.Dtos;

// Player Service Interface

namespace backend.Application.Interfaces.Services;

public interface IPlayerService
{
    //TODO: Remove uneeded, if needed change to save entity when implemented
    
    // // create a player character
    // Task<PlayerCharacterDto> CreatePlayerCharacter(CreatePlayerCharacterDto request);
    //
    // // get a player character by id
    // Task<PlayerCharacterDto> GetPlayerCharacterById(int id);
    //
    // // get all player characters belonging to a user
    // Task<IEnumerable<PlayerCharacterDto>> GetAllPlayerCharactersByUserId(int userId);

    // delete player character
    Task<bool> DeleteCharacter(int id);
    
    // ===== HEALTH MANAGEMENT METHODS =====
    
    /// <summary>
    /// Modify player's health by a specific amount (positive or negative)
    /// </summary>
    Task<int> ModifyHealth(int playerCharacterId, int healthChange);
    
    /// <summary>
    /// Set player's health to a specific value
    /// </summary>
    Task<int> SetHealth(int playerCharacterId, int newHealth);
    
    /// <summary>
    /// Get player's current health
    /// </summary>
    Task<int> GetHealth(int playerCharacterId);
    
    /// <summary>
    /// Check if player is alive (health > 0)
    /// </summary>
    Task<bool> IsAlive(int playerCharacterId);
    
    /// <summary>
    /// Heal player by a specific amount
    /// </summary>
    Task<int> Heal(int playerCharacterId, int healAmount);
    
    /// <summary>
    /// Damage player by a specific amount
    /// </summary>
    Task<int> Damage(int playerCharacterId, int damageAmount);
}