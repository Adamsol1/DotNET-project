using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Logging;
using System;

namespace backend.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository for managing Character entities in the database.
/// </summary>
public class CharacterRepository : GenericRepository<Character>, ICharacterRepository
{

    private readonly AppDbContext _db;
    private readonly IEntityFileLogger _entityLogger;
    
    public CharacterRepository(AppDbContext db, IEntityFileLogger entityLogger) : base(db, entityLogger)
    {
        _db = db;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Get a character associated with given name
    /// The method will return the first character found with the given name.
    /// </summary>

    public async Task<Character?> GetCharacterByName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Character name cannot be null or empty", nameof(name));
            }
            
            return await GetByProperty(c => c.Name, name);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetCharacterByNameError",
                new { Name = name, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    /// <summary>
    /// Get character name with id
    /// The method expects either one or zero results because the character name is unique.
    /// </summary>

    public async Task<string?> GetCharacterNameById(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Character ID must be greater than zero", nameof(id));
            }
            
            return await GetPropertyValue(id, c => c.Name);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetCharacterNameByIdError",
                new { Id = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }



    /// <summary>
    /// Get all characters associated with given name
    /// </summary>

    public async Task<IEnumerable<Character>> GetAllCharactersWithName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Character name cannot be null or empty", nameof(name));
            }
            
            return await GetAllByProperty(c => c.Name, name);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllCharactersWithNameError",
                new { Name = name, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    /// <summary>
    /// Get description of the character given by ID
    /// The method expects either one or zero results because the character description is optional.
    /// </summary>

    public async Task<string?> GetCharacterDescription(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Character ID must be greater than zero", nameof(id));
            }
            
            return await GetPropertyValue(id, c => c.Description);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetCharacterDescriptionError",
                new { Id = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    /// <summary>
    /// Get the URL of the character image given by ID
    /// The method expects either one or zero results because the character image is optional.
    /// </summary>

    public async Task<string?> GetCharacterImageUrl(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Character ID must be greater than zero", nameof(id));
            }
            
            return await GetPropertyValue(id, c => c.ImageUrl);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetCharacterImageUrlError",
                new { Id = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    /// <summary>
    /// Get all dialogues associated with a character given by ID
    /// </summary>
    public async Task<IEnumerable<Dialogue>> GetAllDialoguesOfCharacter(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Character ID must be greater than zero", nameof(id));
            }
            
            var dialogues = await _db.Dialogues
                        .Where(characters => characters.CharacterId == id)
                        .ToListAsync();

            return dialogues;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllDialoguesOfCharacterError",
                new { CharacterId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }
}
