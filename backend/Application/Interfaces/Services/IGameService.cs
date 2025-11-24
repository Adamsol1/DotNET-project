using backend.Domain.Models;
using backend.Application.Dtos;

namespace backend.Application.Interfaces.Services;

/*
The idea is to have a single service that handles the game logic.
it will be used to orchestrate the game,
get the choices, check the progress, and get the story.

*/

public interface IGameService
{

    // Story methods
    Task<StoryNodeDto> GetStoryNodeById(int id);
    Task<IEnumerable<ChoiceDto>> GetChoicesForNode(int storyNodeId);
    Task<GameStateDto> MakeChoiceAsync(int saveId, int choiceId);
    Task<StoryNodeDto?> GetNodeAsync(int nodeId);
    Task<int?> ApplyChoiceAsync(int currentNodeId, int choiceId);

    // Game save methods
    Task<GameSave> CreateGame(int userId, string saveName);
    Task<GameSave> GetGameSave(int gameSaveId);
    Task<IEnumerable<GameSave>> GetUserGameSaves(int userId);
    Task<bool> DeleteGameSave(int gameSaveId);
    Task<GameSave> UpdateGameSave(int gameSaveId, UpdateGameSaveRequest request);




}