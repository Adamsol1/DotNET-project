using backend.Application.Interfaces.Repositories;
using backend.Domain.Models;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Infrastructure.Repositories;

public class GameRepository : GenericRepository<GameSave>, IGameRepository
{
    private readonly AppDbContext _dbContext;

    public GameRepository(AppDbContext context) : base(context) 
    {
        _dbContext = context;
    }

    public async Task<IEnumerable<GameSave>> GetAllByUserId(int userId)
    {
        return await _dbContext.GameSaves
            .Where(gs => gs.UserId == userId)
            .OrderByDescending(gs => gs.LastUpdate)
            .ToListAsync();
    }
}
