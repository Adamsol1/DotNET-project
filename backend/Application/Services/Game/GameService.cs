using System.Text.Json;
using backend.ApplicationNEW.Dtos.Game;
using backend.ApplicationNEW.Dtos.Story;
using backend.ApplicationNEW.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Repositories.Base;

namespace backend.ApplicationNEW.Services.Game;

/*
This service is responsible for handling the game logic.
It will be used to orchestrate the game,
get the choices, check the progress, and get the story.
*/

public class GameService : IGameService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<GameService> _logger;
    private readonly IGenService _genService;

    // constructor
    public GameService(IUnitOfWork uow, ILogger<GameService> logger, IGenService genService)
    {
        _uow = uow;
        _logger = logger;
        _genService = genService;
    }
    
    // Get storyNode by id
    public async Task<StoryNodeDto> GetStoryNodeById(int id)
    {
        try {

            // get the story node from the repository.
            var storyNode = await _uow.StoryNodeRepository.GetById(id);
            if (storyNode == null)
            {
                _logger.LogWarning("gameservice l36: StoryNode with id {id} not found", id);
                throw new Exception("StoryNode not found");
            }

            // return back storyNode Object.
            return new StoryNodeDto {
                Id = storyNode.Id,
                Title = storyNode.Title,
                BackgroundUrl = storyNode.BackgroundUrl,
            };
        } 
        catch (Exception ex)
        {
            // if the try fails, we rollback the transaction.
            _logger.LogError(ex, "GameService - GetStoryNodeById, StoryNode exists, but could not get it", id);
            throw new Exception("gameservice l42: StoryNode exists, but could not get it: " + ex.Message);
        }
    }

    // Get choices for a story node
    // to a list of choice objects.
    public async Task<IEnumerable<ChoiceDto>> GetChoicesForNode(int storyNodeId)
    {
        try {
            var choices = await _uow.ChoiceRepository.GetAllByStoryNodeId(storyNodeId);
            if (choices == null){
                _logger.LogError("gameservice l64: could not find choices for story node with id: {storyNodeId}", storyNodeId);
                throw new Exception("gameservice l64: could not find choices for story node with id: " + storyNodeId);
            }
            
            // return the choices into a dto object.
            return choices.Select(c => new ChoiceDto {
                Id = c.Id,
                Text = c.Text,
                StoryNodeId = c.StoryNodeId,
                NextStoryNodeId = c.NextStoryNodeId,
 				AudioUrl = c.AudioUrl
            }).ToList();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gameservice l79: failed to get choice");
            throw new Exception("gameservice l79: failed to get choice: " + ex.Message);
        }
    }
    
    // NOTE: Choice handling is now consolidated in StoryControllerService.
    // Remove duplicated MakeChoiceAsync and related methods from GameService.

    /// <summary>
    /// Create a new game save for a user
    /// </summary>
    public async Task<GameSave> CreateGame(int userId, string saveName)
    {
        try
        {
            await _uow.BeginAsync();
            
            var user = await _uow.UserRepository.GetById(userId);
            if (user == null) throw new Exception("gameservice: user not found, Check that you are not passing the auth ID!");

            var playerCharacter = await _uow.PlayerCharacterRepository.GetById(1);

            // create the game save object.
            var gameSave = new GameSave
            {
                UserId = userId,
                SaveName = saveName,
                PlayerCharacterId = playerCharacter.Id,
                CurrentStoryNodeId = 1,
                LastUpdate = DateTime.UtcNow,
                Health = 100,
            };

            // set the player character health to 100 on new game.
            playerCharacter.Health = gameSave.Health;
            await _uow.PlayerCharacterRepository.Update(playerCharacter);
            var playerCharacter2 = await _uow.PlayerCharacterRepository.GetById(1);


            // create the game save in the repository.
            await _uow.GameRepository.Create(gameSave);


            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return the game save object.
            return gameSave;
        }
        catch (Exception ex)
        {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "gameservice l156: failed to create game save");
            throw new Exception("gameservice l156: failed to create game save: " + ex.Message);
        }
    }

    public async Task<GameSave> GetGameSave(int gameSaveId)
    {
        try {
            // get the gameSave Object that they requested.
            var gameSave = await _uow.GameRepository.GetById(gameSaveId);
            if (gameSave == null) throw new Exception("gameservice l177: game save not found");

            return gameSave;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gameservice l181: failed to get game save");
            throw new Exception("gameservice l181: failed to get game save: " + ex.Message);
        }
    }

    // get all the game saves for a user.
    public async Task<IEnumerable<GameSave>> GetUserGameSaves(int userId)
    {
        try {
            var gameSaves = await _uow.GameRepository.GetAllByUserId(userId);

            if (gameSaves == null) throw new Exception("gameservice l192: no game saves found for user");

            // return the game saves as a list of objects.
            return gameSaves.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gameservice l198: failed to get user game saves");
            throw new Exception("gameservice l198: failed to get user game saves: " + ex.Message);
        }
    }

    public async Task<GameSave> UpdateGameSave(int gameSaveId, UpdateGameSaveRequest request)
    {
        try {
            await _uow.BeginAsync();
    
            var gameSave = await _uow.GameRepository.GetById(gameSaveId);
            if (gameSave == null) 
                throw new Exception("gameservice: game save not found");
    
            if (request.CurrentStoryNodeId.HasValue)
            {
                gameSave.CurrentStoryNodeId = request.CurrentStoryNodeId.Value;
            }

            // Merge instead of overwriting
            if (request.VisitedNodeIds != null && request.VisitedNodeIds.Any())
            {
                var existing = string.IsNullOrEmpty(gameSave.VisitedNodeIds)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(gameSave.VisitedNodeIds) ?? new List<int>();

                foreach (var id in request.VisitedNodeIds)
                {
                    if (!existing.Contains(id))
                        existing.Add(id);
                }

                gameSave.VisitedNodeIds = JsonSerializer.Serialize(existing);
            }

            gameSave.LastUpdate = DateTime.UtcNow;

            await _uow.GameRepository.Update(gameSave);
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return gameSave;
        }
        catch (Exception ex)
        {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "gameservice: failed to update game save");
            throw new Exception("gameservice: failed to update game save: " + ex.Message);
        }
    }

    // delete a game save.
    public async Task<bool> DeleteGameSave(int gameSaveId)
    {
        try {
            // start a transaction
            await _uow.BeginAsync();
            
            // delete the game save from the repository.
            await _uow.GameRepository.Delete(gameSaveId);

            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            // if the try fails, we rollback the transaction.
            await _uow.RollBackAsync();

            // and give the user an error message.
            _logger.LogError(ex, "gameservice l208: failed to delete game save");
            throw new Exception("gameservice l208: failed to delete game save: " + ex.Message);
        }
    }



    
    // Helper method to create GameSaveDto
    private GameSaveDto returnGameDto(GameSave gameSave)
    {
        return new GameSaveDto {
            Id = gameSave.Id,
            UserId = gameSave.UserId,
            SaveName = gameSave.SaveName,
            PlayerCharacterId = gameSave.PlayerCharacterId,
            CurrentStoryNodeId = gameSave.CurrentStoryNodeId,
            LastUpdate = gameSave.LastUpdate
        };
    }




    // Additional methods for story navigation
    public async Task<StoryNodeDto?> GetNodeAsync(int nodeId)
    {
        var storyNode = await _uow.StoryNodeRepository.GetById(nodeId);
        if (storyNode == null)
        {
            _logger.LogWarning("[Gameservice] StoryNode with id {StoryNodeId} not found", nodeId);
            return null;
        }

        var dialogues = await _uow.StoryNodeRepository.GetAllDialoguesOfStoryNode(storyNode.Id);
        var choices = await _uow.StoryNodeRepository.GetAllChoicesOfStoryNode(storyNode.Id);

        return new StoryNodeDto
        {
            Id = storyNode.Id,
            Title = storyNode.Title,
            Description = storyNode.Description,
            BackgroundUrl = storyNode.BackgroundUrl,
            BackgroundMusicUrl = storyNode.BackgroundMusicUrl,
            AmbientSoundUrl = storyNode.AmbientSoundUrl,

            Dialogues = dialogues
                .OrderBy(d => d.Order)
                .Select(d => new DialogueDto
                {
                    Id = d.Id,
                    Text = d.Text,
                    CharacterId = d.CharacterId,
                    Order = d.Order
                }).ToList(),
            Choices = choices
                .Select(c => new ChoiceDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    StoryNodeId = c.StoryNodeId,
                    NextStoryNodeId = c.NextStoryNodeId,
                    AudioUrl = c.AudioUrl
                }).ToList()
        };
    }
    
    /// <summary>
    /// Applies a choice to the current story node and returns the next node ID.
    /// </summary>
    public async Task<int?> ApplyChoiceAsync(int currentNodeId, int choiceId)
    {
        // Validate that choice belongs to currentNodeId
        var choice = await _uow.ChoiceRepository.GetById(choiceId);
        if (choice == null)
        {
            _logger.LogWarning("[Gameservice] Choice with id {ChoiceId} not found", choiceId);
            return null;
        }
        if (choice.StoryNodeId != currentNodeId)
        {
            _logger.LogError("[Gameservice] Choice with id {ChoiceId} does not belong to {CurrentNodeId}", choiceId, currentNodeId);
            throw new InvalidOperationException("Choice does not belong to current node.");
        }

        // Return next node ID (can be null for end)
        return choice.NextStoryNodeId;
    }
}

