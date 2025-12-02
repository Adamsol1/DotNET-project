using Microsoft.AspNetCore.Mvc;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using Microsoft.AspNetCore.Authorization;


namespace backend.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService; 

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
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
                
                return BadRequest("Failed to create game save");
            }
            

            // return the game save.
            return Ok(gameSave);
        } catch (Exception) {
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
            return Ok(gameSave);
        } catch (Exception) {
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
            return Ok(gameSaves);
        } catch (Exception ex) {
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
                return NotFound("Failed to delete game save");
            }

            return Ok(new { message = "Save deleted successfully" });

        } catch (Exception) {
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
            return Ok(gameSave);
        } catch (Exception ex) {
            return BadRequest($"Failed to update game save: {ex.Message}");
        }
    }
}


