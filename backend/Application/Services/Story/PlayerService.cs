
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;

namespace backend.Application.Services.Story;


/// <summary>
/// Service for handling player related operation in the story. 
/// Uses a unit of work for data access, and a logger for logging operations/errors. 
/// </summary>

public class PlayerService

{
    //The unit of work
    private readonly IUnitOfWork _uow;
    //The logger
    private readonly IEntityFileLogger _logger;

    public PlayerService(IUnitOfWork uow, IEntityFileLogger logger)
    {
        _uow = uow;
        _logger = logger;
    }

}