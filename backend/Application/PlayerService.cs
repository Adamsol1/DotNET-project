using backend.Application.Dtos;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Domain.Models;
using Serilog;

namespace backend.Application;

public class PlayerService : IPlayerService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PlayerService> _logger;

    // constructor
    public PlayerService(IUnitOfWork uow, ILogger<PlayerService> logger)
    {
        _logger = logger;
        _uow = uow;
    }
    //
    // // create a player character
    // public async Task<PlayerCharacterDto> CreatePlayerCharacter(CreatePlayerCharacterDto request) 
    // {
    //     try {
    //         // begin transaction
    //         await _uow.BeginAsync();
    //
    //         // create player character
    //         var playerCharacter = new PlayerCharacter {
    //             Name = request.Name,
    //             UserId = request.UserId,
    //             CurrentStoryNodeId = request.CurrentStoryNodeId,
    //             Health = 100 // Default health
    //         };
    //
    //         // add player character to database
    //         await _uow.PlayerCharacterRepository.Create(playerCharacter);
    //         await _uow.SaveAsync();
    //         await _uow.CommitAsync();
    //
    //         return ReturnPlayerCharacterDto(playerCharacter);
    //
    //     } catch (Exception ex) {
    //         await _uow.RollBackAsync();
    //         throw new Exception($"Failed to create player character: {ex.Message}");
    //     }
    // }


    //TODO: Again change to save when implemented

    // get a player character by id
    // public async Task<PlayerCharacterDto> GetPlayerCharacterById(int id) 
    // {
    //     try {
    //         // begin transaction
    //         await _uow.BeginAsync();
    //
    //         // get player character by id
    //         var playerCharacter = await _uow.PlayerCharacterRepository.GetById(id);
    //
    //         // if player character is not found, throw an exception
    //         if (playerCharacter == null) {
    //             throw new Exception("Player character not found");
    //         }
    //
    //         // return player character
    //         return ReturnPlayerCharacterDto(playerCharacter);
    //     } catch (Exception ex) {
    //         await _uow.RollBackAsync();
    //         throw new Exception($"Failed to get player character: {ex.Message}");
    //     }
    // }
    //TODO change to save when save is implemented
    // get all player characters belonging to a user
    // public async Task<IEnumerable<PlayerCharacterDto>> GetAllPlayerCharactersByUserId(int userId) 
    // {
    //     try {
    //         // begin transaction
    //         await _uow.BeginAsync();
    //         
    //         //TODO: Fix to save entity later on when save is added as an entity
    //         //
    //         // get all player characters by user id 
    //         //var playerCharacters = await _uow.PlayerCharacterRepository.GetAllByUserId(userId);
    //
    //         // if player characters are not found, throw an exception
    //         // if (playerCharacters == null) {
    //         //     throw new Exception("Player characters not found");
    //         // }
    //
    //         // // return player characters
    //         // return playerCharacters.Select(ReturnPlayerCharacterDto);
    //     }
    //     catch (Exception ex) {
    //         await _uow.RollBackAsync();
    //         throw new Exception($"Failed to get player characters: {ex.Message}");
    //     }
    // }

    // delete player character
    public async Task<bool> DeleteCharacter(int id)
    {
        try
        {
            // begin transaction
            await _uow.BeginAsync();

            // check the player character in the database
            var playerCharacter = await _uow.PlayerCharacterRepository.GetById(id);

            // if player character is not found, throw an exception
            if (playerCharacter == null)
            {
                _logger.LogWarning("[Playerservice] Player character with id {ID} not found", id);
                return false;
            }

            // delete player character
            await _uow.PlayerCharacterRepository.Delete(id);
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Playerservice] Error deleting player character with id {id}", id);
            throw;
        }
    }

    // Set player's health to a specific value
    public async Task<int> SetHealth(int playerCharacterId, int newHealth)
    {
        try
        {
            await _uow.BeginAsync();

            // get the player from repository
            var playerCharacter = await _uow.PlayerCharacterRepository.GetById(playerCharacterId);
            if (playerCharacter == null)
            {
                _logger.LogWarning("[Playerservice] Player character with playerCharacterId {playerCharacterId} not found", playerCharacterId);
                throw new Exception("Player character not found");
            }

            // set health if remove use remove method
            playerCharacter.Health = Math.Max(0, newHealth);

            // update the player
            await _uow.PlayerCharacterRepository.Update(playerCharacter);
            // save & commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return playerCharacter.Health;
        }
        catch (Exception ex)
        {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Playerservice] Error setting health to Player character with playerCharacterId {playerCharacterId}", playerCharacterId);
            throw;
        }
    }

    // Get player's current health
    public async Task<int> GetHealth(int playerCharacterId)
    {
        try
        {

            // get the player
            var playerCharacter = await _uow.PlayerCharacterRepository.GetById(playerCharacterId);
            if (playerCharacter == null)
            {
                _logger.LogWarning("[Playerservice] Player character with playerCharacterId {playerCharacterId} not found", playerCharacterId);
                throw new Exception("Player character not found");
            }

            return playerCharacter.Health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Playerservice] Error retrieving health for Player character with playerCharacterId {playerCharacterId}", playerCharacterId);
            throw;
        }
    }

    // Check if player is alive (health > 0)
    public async Task<bool> IsAlive(int playerCharacterId)
    {
        try
        {
            var health = await GetHealth(playerCharacterId);
            return health > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Playerservice] Error checking Player characther with playerCharacterId {playerCharacterid} is alive", playerCharacterId);
            throw;
        }
    }

    // Modify player's health by a specific amount (positive or negative)
    public async Task<int> ModifyHealth(int playerCharacterId, int healthChange)
    {
        try
        {
            await _uow.BeginAsync();

            var playerCharacter = await _uow.PlayerCharacterRepository.GetById(playerCharacterId);
            if (playerCharacter == null)
            {
                _logger.LogWarning("[PlayerService] Player character with playerCharacterId {playerCharacterId} not found", playerCharacterId);
                throw new Exception("Player character not found");
            }

            // modify health (ensure it doesn't go below 0)
            playerCharacter.Health = Math.Max(0, playerCharacter.Health + healthChange);

            await _uow.PlayerCharacterRepository.Update(playerCharacter);
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return playerCharacter.Health;
        }
        catch (Exception ex)
        {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Playerservice] Error modifying health for Playercharacter with playerCharacterId {playerCharacterId}", playerCharacterId);
            throw;
        }
    }

    // Heal player by a specific amount
    public async Task<int> Heal(int playerCharacterId, int healAmount)
    {
        return await ModifyHealth(playerCharacterId, healAmount);
    }

    // Damage player by a specific amount
    public async Task<int> Damage(int playerCharacterId, int damageAmount)
    {
        return await ModifyHealth(playerCharacterId, -damageAmount);
    }


//     // Helper method to convert PlayerCharacter to DTO
//     private static PlayerCharacterDto ReturnPlayerCharacterDto(PlayerCharacter playerCharacter) => new PlayerCharacterDto
//     {
//         Id = playerCharacter.Id,
//         Name = playerCharacter.Name,
//         Health = playerCharacter.Health,
//         UserId = playerCharacter.UserId,
//         CurrentStoryNodeId = playerCharacter.CurrentStoryNodeId
//     };
    
}