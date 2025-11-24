using backend.Domain.Models;

namespace backend.Application.Interfaces.Repositories;

public interface IGameRepository : IGenericRepository<GameSave>
{
    Task<IEnumerable<GameSave>> GetAllByUserId(int userId);
}
