using backend.Domain.Models;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Logging;
using System;

namespace backend.Infrastructure.Repositories;

/// <summary>
/// Repository for managing PlayerCharacter entities in the database.
/// 
/// All base crud operations are inherited from the GenericRepository class.
/// </summary>
public class PlayerCharacterRepository : GenericRepository<PlayerCharacter>, IPlayerCharacterRepository
{
    private readonly AppDbContext _db;
    private readonly IEntityFileLogger _entityLogger;

    public PlayerCharacterRepository(AppDbContext db, IEntityFileLogger entityLogger) : base(db, entityLogger)
    {
        _db = db;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Get health of a player character by its ID.
    /// </summary>
    public async Task<int> GetHealthByIdAsync(int id)
    {
        try {
            /// Query to get health of a player character by its ID.
        var health = _db.Characters.OfType<PlayerCharacter>()
                    .Where(PlayerCharacter => PlayerCharacter.Id == id)
                    .Select(PlayerCharacter => PlayerCharacter.Health)
                    .SingleOrDefaultAsync();

            return await health;
            
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "GetHealthByIdAsyncError", 
                new { 
                    Id = id,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

}