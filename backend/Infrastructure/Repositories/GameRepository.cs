using backend.Application.Interfaces.Repositories;
using backend.Domain.Models;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Infrastructure.Repositories;

/// <summary>
/// Repository for managing GameSave entities in the database.
/// </summary>
public class GameRepository : GenericRepository<GameSave>, IGameRepository
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Constructor for GameRepository
    /// </summary>
    public GameRepository(AppDbContext context) : base(context)
    {
        _dbContext = context;
    }

    /// <summary>
    /// Get all game saves for a specific user
    /// </summary>
    public async Task<IEnumerable<GameSave>> GetAllByUserId(int userId)
    {
        return await _dbContext.GameSaves
            .Where(gs => gs.UserId == userId)
            .OrderByDescending(gs => gs.LastUpdate)
            .ToListAsync();
    }
}
