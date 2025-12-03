using backend.Application.Dtos.Game;
using backend.Application.Interfaces;
using backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Game;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly IEntityFileLogger _entityLogger;

    public GameController(IGameService gameService, IEntityFileLogger entityFileLogger)
    {
        _gameService = gameService;
        _entityLogger = entityFileLogger;
    }

    // start a new game from scratch.
    // takes inn a request from the frontend to start a new game.
    [HttpPost("start")]
        public async Task<ActionResult<GameSaveDto>> Start(StartGameRequestDto request)
    {
        try {
            // create a new game save.
            var gameSave = await _gameService.CreateGame(request.UserId, request.SaveName ?? string.Empty);

            if (gameSave == null) {
                // Log failure to create game
                await _entityLogger.LogAsync(
                    "start game error",
                    new
                    {
                        UserId = request.UserId,
                        SaveName = request.SaveName,
                        Timestamp = DateTime.UtcNow
                    },
                    LogCategories.Game
                );
                return BadRequest("Failed to create game save");
            }
            
            // Log successful game creation
            await _entityLogger.LogAsync(
                "start game successful",
                gameSave,
                LogCategories.Game
            );

            // return the game save.
            return Ok(gameSave);
        } catch (Exception ex) {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "start game error",
                new
                {
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return BadRequest($"Failed to start game");
        }
    }

    // load a game save.
    [HttpGet("load/{saveId}")]
    public async Task<ActionResult<GameSaveDto>> LoadGame(int saveId)
    {
        try {
            // load the game save.
            var gameSave = await _gameService.GetGameSave(saveId);
            
            // Log successful load
            await _entityLogger.LogAsync(
                "load game successful",
                gameSave,
                LogCategories.Game
            );
            
            return Ok(gameSave);
        } catch (KeyNotFoundException ex) {
            // Log game save not found
            await _entityLogger.LogAsync(
                "load game error not found",
                new
                {
                    SaveId = saveId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return NotFound($"Failed to load game");
        } catch (Exception ex) {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "load game error",
                new
                {
                    SaveId = saveId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return BadRequest($"Failed to load game");
        }
    }

    // NOTE: Choice handling is served by StoryController; GameController no longer exposes /game/choice

    // Get user's saved games
    [HttpGet("saves/{userId}")]
    public async Task<ActionResult<IEnumerable<GameSaveDto>>> GetUserSaves(int userId)
    {
        try {
            // get the user's saved games.
            var gameSaves = await _gameService.GetUserGameSaves(userId);
            
            // Log successful retrieval
            await _entityLogger.LogAsync(
                "get user saves successful",
                new
                {
                    UserId = userId,
                    SaveCount = gameSaves.Count(),
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            
            return Ok(gameSaves);
        } catch (KeyNotFoundException ex) {
            // Log user not found
            await _entityLogger.LogAsync(
                "get user saves error not found",
                new
                {
                    UserId = userId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return NotFound($"Failed to get user's saved games: {ex.Message}");
        } catch (Exception ex) {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "get user saves error",
                new
                {
                    UserId = userId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return BadRequest($"Failed to get user's saved games: {ex.Message}");
        }
    }

    // Delete a game save
    [HttpDelete("saves/{saveId}")]
    public async Task<ActionResult<bool>> DeleteGameSave(int saveId)
    {
        try {
            // delete the game save.
            var result = await _gameService.DeleteGameSave(saveId);

            if(!result) {
                // Log deletion failure
                await _entityLogger.LogAsync(
                    "delete game save error",
                    new
                    {
                        SaveId = saveId,
                        Timestamp = DateTime.UtcNow
                    },
                    LogCategories.Game
                );
                return NotFound("Failed to delete game save");
            }

            // Log successful deletion
            await _entityLogger.LogAsync(
                "delete game save successful",
                new
                {
                    SaveId = saveId,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );

            return Ok(new { message = "Save deleted successfully" });

        } catch (KeyNotFoundException ex) {
            // Log save not found
            await _entityLogger.LogAsync(
                "delete game save error not found",
                new
                {
                    SaveId = saveId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return NotFound("Failed to delete game save");
        } catch (Exception ex) {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "delete game save error",
                new
                {
                    SaveId = saveId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return BadRequest($"Failed to delete game save");
        }
    }

    // Update a game save
     [HttpPut("saves/{saveId}")]
    public async Task<ActionResult<GameSaveDto>> UpdateGameSave(
        int saveId, 
        [FromBody] UpdateGameSaveRequest request)
    {
        try {
            // update the game save.
            var gameSave = await _gameService.UpdateGameSave(saveId, request);
            
            // Log successful update
            await _entityLogger.LogAsync(
                "update game save successful",
                gameSave,
                LogCategories.Game
            );
            
            return Ok(gameSave);
        } catch (KeyNotFoundException ex) {
            // Log save not found
            await _entityLogger.LogAsync(
                "update game save error not found",
                new
                {
                    SaveId = saveId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return NotFound($"Failed to update game save: {ex.Message}");
        } catch (Exception ex) {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "update game save error",
                new
                {
                    SaveId = saveId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Game
            );
            return BadRequest($"Failed to update game save: {ex.Message}");
        }
    }
}


