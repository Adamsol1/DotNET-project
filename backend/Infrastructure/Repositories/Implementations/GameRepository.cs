using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository for managing GameSave entities in the database.
/// 
/// All base crud operations are inherited from the GenericRepository class.
/// </summary>
public class GameRepository : GenericRepository<GameSave>, IGameRepository
{
    private readonly AppDbContext _dbContext;
    private readonly IEntityFileLogger _entityLogger;
    /// <summary>
    /// Constructor for GameRepository
    /// </summary>
    public GameRepository(AppDbContext context, IEntityFileLogger entityLogger) : base(context, entityLogger)
    {
        _dbContext = context;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Get all game saves for a specific user
    /// </summary>
    public async Task<IEnumerable<GameSave>> GetAllByUserId(int userId)
    {
        try {

            return await _dbContext.GameSaves
            .Where(gs => gs.UserId == userId)
            .OrderByDescending(gs => gs.LastUpdate)
            .ToListAsync();

        } catch (Exception ex) {

            await _entityLogger.LogAsync(
                "GetAllByUserIdError", 
                new { 
                    UserId = userId,
                    Reason = ex.Message,
                    Timestamp = DateTime.UtcNow
                    }, 
                LogCategories.System);
            throw;
        }
    }
}
