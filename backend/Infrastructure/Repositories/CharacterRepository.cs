using backend.Domain.Models;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories;

public class CharacterRepository : GenericRepository<Character>, ICharacterRepository
{

    private readonly AppDbContext _db;
    public CharacterRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    /// <summary>
    /// Get a character associated with given name
    /// The method will return the first character found with the given name.
    /// </summary>

    public async Task<Character?> GetCharacterByName(string name)
    {
        return await GetByProperty(c => c.Name, name);
    }


    /// <summary>
    /// Get character name with id
    /// The method expects either one or zero results because the character name is unique.
    /// </summary>

    public async Task<string> GetCharacterNameById(int id)
    {
        return await GetPropertyValue(id, c => c.Name);

    }



    /// <summary>
    /// Get all characters associated with given name
    /// </summary>

    public async Task<IEnumerable<Character>> GetAllCharactersWithName(string name)
    {
        return await GetAllByProperty(c => c.Name, name);
    }


    /// <summary>
    /// Get description of the character given by ID
    /// The method expects either one or zero results because the character description is optional.
    /// </summary>

    public async Task<string> GetCharacterDescription(int id)
    {
        return await GetPropertyValue(id, c => c.Description);
    }


    /// <summary>
    /// Get the URL of the character image given by ID
    /// The method expects either one or zero results because the character image is optional.
    /// </summary>

    public async Task<string> GetCharacterImageUrl(int id)
    {
        return await GetPropertyValue(id, c => c.ImageUrl);
    }


    /// <summary>
    /// Get all dialogues associated with a character given by ID
    /// </summary>


    public async Task<IEnumerable<Dialogue>> GetAllDialoguesOfCharacter(int id)
    {
        /// Query to get all dialogues associated with a character given by ID
        var dialogues = await _db.Dialogues
                    .Where(characters => characters.CharacterId == id)
                    .ToListAsync();
        
        return dialogues;
    }
}
