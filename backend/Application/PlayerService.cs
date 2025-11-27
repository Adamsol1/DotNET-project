// PlayerService has been trimmed: methods previously unused were removed.
// Keep a minimal service class to avoid breaking existing DI/tests while
// removing unused public methods per cleanup request.
using backend.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace backend.Application;

public class PlayerService

{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(IUnitOfWork uow, ILogger<PlayerService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

}