using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using backend.Domain.Models;
using System.Text.Json;

namespace backend.Application;

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
    
    public async Task<GameStateDto> MakeChoiceAsync(int saveId, int choiceId) 
    {
        try
        {
            // start a transaction as the user does
            // write interaction with the db.
            await _uow.BeginAsync();

            // get the game save
            var gameSave = await _uow.GameRepository.GetById(saveId);
            if (gameSave == null) throw new Exception("gameservice: game save not found");

            // get the choice from the repository.
            var choice = await _uow.ChoiceRepository.GetById(choiceId);
            if (choice == null) throw new Exception("gameservice: choice not found");
            

            // get the next story node by the choice's next story node id.
            var nextStoryNode = await _uow.StoryNodeRepository.GetById(choice.NextStoryNodeId);
            if (nextStoryNode == null) throw new Exception("gameservice: next story node not found");
            
            var playerCharacter = await _uow.PlayerCharacterRepository.GetById(gameSave.PlayerCharacterId);
            if (playerCharacter == null) throw new Exception("playerCharacter: next story node not found");


            //TODO: See if we could call the modifyhealth from PlayerService instead
            if (choice.HealthEffect.HasValue)
            {
                playerCharacter.Health += choice.HealthEffect.Value;

                // Clamp health between 0 and 100   
                if (playerCharacter.Health > 100) playerCharacter.Health = 100;
                if (playerCharacter.Health < 0) playerCharacter.Health = 0;

                await _uow.PlayerCharacterRepository.Update(playerCharacter);
            }
            
            // deserialize visited nodes
            var visitedNodes = JsonSerializer.Deserialize<List<int>>(gameSave.VisitedNodeIds) 
                               ?? new List<int>();
            
            // if the visited nodes list is empty or the last node is not the current node, 
            // add the current node to the list.
            if (!visitedNodes.Contains(gameSave.CurrentStoryNodeId))
            {
                visitedNodes.Add(gameSave.CurrentStoryNodeId);
            }

            var choices = await _uow.StoryNodeRepository.GetAllChoicesOfStoryNode(gameSave.CurrentStoryNodeId);

            // map domain Choice -> ChoiceDto using GenService
            var availableChoices = choices.Select(c => _genService.MapChoice(c)).ToList();

            
            
            // update the game save with the new story node
            gameSave.CurrentStoryNodeId = choice.NextStoryNodeId;
            gameSave.Health = playerCharacter.Health;
            gameSave.VisitedNodeIds = JsonSerializer.Serialize(visitedNodes);
            gameSave.LastUpdate = DateTime.UtcNow;
            gameSave.CurrentDialogueIndex = 0;
            await _uow.GameRepository.Update(gameSave);

            
            var nextNodeDto = await GetNodeAsync(gameSave.CurrentStoryNodeId);

            // check if game over
            var isGameOver = playerCharacter.Health <= 0;

            // prepare the game state dto to return
            var playerDto = new PlayerCharacterDto
            {
                Id = playerCharacter.Id,
                Name = playerCharacter.Name,
                Health = playerCharacter.Health,
            };
            
            // // prepare the game state dto to return
            var gameState = new GameStateDto
            {
                SaveId = gameSave.Id,
                PlayerCharacter = playerDto,
                CurrentStoryNode = nextNodeDto,
                AvailableChoices = availableChoices,
                IsGameOver = isGameOver
            };
            
            
            // save the changes, and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // finally return the next story node.
            return gameState;
        }
        catch (Exception ex)
        {
            // if the try fails, we rollback the transaction.
            await _uow.RollBackAsync();

            // and give the user an error message.
            _logger.LogError(ex, "gameservice: failed to make choice");
            throw new Exception("gameservice: failed to make choice: " + ex.Message);
        }
    }

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

