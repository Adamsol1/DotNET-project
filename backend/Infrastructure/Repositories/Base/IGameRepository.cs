using backend.Domain.Models;

namespace backend.Infrastructure.Repositories.Base;

public interface IGameRepository : IGenericRepository<GameSave>
{
    Task<IEnumerable<GameSave>> GetAllByUserId(int userId);
}
