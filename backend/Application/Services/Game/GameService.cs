using System.Text.Json;
using backend.Application.Dtos.Game;
using backend.Application.Dtos.Story;
using backend.Application.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;

namespace backend.Application.Services.Game;

/*
This service is responsible for handling the game logic.
It will be used to orchestrate the game,
get the choices, check the progress, and get the story.
*/

public class GameService : IGameService
{
    // Dependencies and logger
    private readonly IUnitOfWork _uow;
    private readonly IEntityFileLogger _entityLogger;
    private readonly IGenService _genService;

    // Constructor for the GameService class.
    public GameService(IUnitOfWork uow, IGenService genService, IEntityFileLogger entityLogger)
    {
        _uow = uow;
        _genService = genService;
        _entityLogger = entityLogger;
    }


    // Get storyNode by id
    /// <summary>
    /// Method for getting a story node by its id. 
    /// The method will log the process and handle exceptions.
    /// </summary>
    /// <param name="id">The id of the story node</param>
    /// <returns>The story node</returns>
    /// <exception cref="Exception">Errors occured while getting the story node.</exception>
    public async Task<StoryNodeDto> GetStoryNodeById(int id)
    {
        try
        {
            // get the story node from the repository by id.
            var storyNode = await _uow.StoryNodeRepository.GetById(id);
            //Checks if the story node is null. If it is, throw an exception.
            if (storyNode == null)
            {
                throw new Exception("StoryNode not found");
            }

            // return back storyNode Object.
            return new StoryNodeDto
            {
                Id = storyNode.Id,
                Title = storyNode.Title,
                BackgroundUrl = storyNode.BackgroundUrl,
            };
        }
        catch (Exception ex)
        {
            // if the try fails, we rollback the transaction
            // if the try fails, we rollback the transaction.
        
            throw new Exception("gameservice l42: StoryNode exists, but could not get it: " + ex.Message);
        }
    }


    /// <summary>
    /// Method for getting choices for a story node by its id. 
    /// The method will return a list of choice DTOs.
    /// </summary>
    /// <param name="storyNodeId">The id of the story node</param>
    /// <returns>Returns a list of choice DTOs</returns>
    /// <exception cref="Exception">All errors related to choice retrieval</exception>
    public async Task<IEnumerable<ChoiceDto>> GetChoicesForNode(int storyNodeId)
    {
        try
        {
            //Get all choices for the story node
            var choices = await _uow.ChoiceRepository.GetAllByStoryNodeId(storyNodeId);
            //If the choices are null, throw an exception.
            if (choices == null)
            {
                throw new Exception("gameservice l64: could not find choices for story node with id: " + storyNodeId);
            }

            // return the choices into a dto object.
            return choices.Select(c => new ChoiceDto
            {
                Id = c.Id,
                Text = c.Text,
                StoryNodeId = c.StoryNodeId,
                NextStoryNodeId = c.NextStoryNodeId,
                AudioUrl = c.AudioUrl
            }).ToList();

        }
        catch (Exception ex)
        {
            // Throw error
            throw new Exception("gameservice l79: failed to get choice: " + ex.Message);
        }
    }


    /// <summary>
    /// Method for creating a new game save for user. Takes in a userId and saveName that the user chooses. 
    /// The method will contain the repositories to create the game save and log the process.
    /// <param name="userId">The id of the user creating the game save</param>
    /// <param name="saveName">The name of the save that the user chooses</param>
    /// <returns>Returns the created GameSave object</returns>
    /// </summary>
    public async Task<GameSave> CreateGame(int userId, string saveName)
    {
        try
        {
            // start a transaction
            await _uow.BeginAsync();
            
            var user = await _uow.UserRepository.GetById(userId);
            if (user == null) throw new Exception("gameservice: user not found, Check that you are not passing the auth ID!");

            var existingSaves = await _uow.GameRepository.GetAllByUserId(userId);
            var saveCount = existingSaves.Count();
            if (saveCount >= 3)
            {
                throw new Exception("Maximum 3 saves reached. Please delete an existing save before creating a new one.");
            }

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
            // if the try fails, we rollback the transaction
            await _uow.RollBackAsync();
            throw new Exception("gameservice: failed to create game save: " + ex.Message);
        }
    }

    /// <summary>
    /// Method used to get a game save by its id. 
    /// </summary>
    /// <param name="gameSaveId">The id of the gamesave being requested.</param>
    /// <returns>A game save object</returns>
    /// <exception cref="Exception">Exception thrown when the game save is not found.</exception>
    public async Task<GameSave> GetGameSave(int gameSaveId)
    {
        try
        {
            // get the gameSave Object that they requested.
            var gameSave = await _uow.GameRepository.GetById(gameSaveId);
            //If not found throw an exception.
            if (gameSave == null)
            {
                throw new Exception("gameservice: game save not found");
            }
            //return the game save object.
            return gameSave;
        }
        catch (Exception ex)
        {
            //throw the exception
            throw new Exception("gameservice: failed to get game save: " + ex.Message);
        }
    }

    // get all the game saves for a user.
    /// <summary>
    /// Method used to get all game saves for a user by their user id.
    /// The method will return a list of game saves.
    /// </summary>
    /// <param name="userId">The user id of the player</param>
    /// <returns>A list of all gamesaves</returns>
    /// <exception cref="Exception"></exception>
    public async Task<IEnumerable<GameSave>> GetUserGameSaves(int userId)
    {
        try
        {
            //Attempts to get all game saves for the user with given id.
            var gameSaves = await _uow.GameRepository.GetAllByUserId(userId);
            //If no game saves are found, throw an exception.
            if (gameSaves == null)
            {
                throw new Exception("gameservice l192: no game saves found for user");
            }

            // return the game saves as a list of objects.
            return gameSaves.ToList();
        }
        catch (Exception ex)
        {
            //throw the exception
            throw new Exception("gameservice l198: failed to get user game saves: " + ex.Message);
        }
    }

/// <summary>
/// Method used to update a game save with new data. 
/// </summary>
/// <param name="gameSaveId">The game save that is being updated</param>
/// <param name="request">The request with the updated game save data</param>
/// <returns>The updated game save</returns>
/// <exception cref="Exception"></exception>
    public async Task<GameSave> UpdateGameSave(int gameSaveId, UpdateGameSaveRequest request)
    {
        try
        {
            // start a transaction
            await _uow.BeginAsync();
            //Get the game save from repository by id.
            var gameSave = await _uow.GameRepository.GetById(gameSaveId);
            //Check for null. If null, throw exception.
            if (gameSave == null)
            {
                throw new Exception("gameservice: game save not found");
            }
                
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

            // Update the last update time
            gameSave.LastUpdate = DateTime.UtcNow;

            //Update the save in the repository.
            await _uow.GameRepository.Update(gameSave);
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            //Return the updated game save.
            return gameSave;
        }
        catch (Exception ex)
        {
            // If the try fails, rollback transaction
            await _uow.RollBackAsync();
            //throw the exception
            throw new Exception("gameservice: failed to update game save: " + ex.Message);
        }
    }

    /// <summary>
    /// Method used to delete a game save by its id. 
    /// This method will handle the transaction and rollback if it fails to prevent issues. 
    /// The method return a boolean indicating whether the deletion was successful.
    /// </summary>
    /// <param name="gameSaveId">The id of the save that is being deleted</param>
    /// <returns>Return a boolean indicating whether the deletion was successful</returns>
    /// <exception cref="Exception">The error that occurred during deletion</exception>
    public async Task<bool> DeleteGameSave(int gameSaveId)
    {
        try
        {
            // start a transaction
            await _uow.BeginAsync();

            // delete the game save from the repository with given id
            await _uow.GameRepository.Delete(gameSaveId);

            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return true if successful
            return true;
        }
        catch (Exception ex)
        {
            // if the try fails, we rollback the transaction.
            await _uow.RollBackAsync();
            //throw the exception
            throw new Exception("gameservice: failed to delete game save: " + ex.Message);
        }
    }





    /// <summary>
    /// Helper method to convert Gamesave to a GameSaveDto
    /// </summary>
    /// <param name="gameSave">The GameSave entity</param>
    /// <returns>A gamesave DTO</returns>
    private GameSaveDto returnGameDto(GameSave gameSave)
    {
        return new GameSaveDto
        {
            Id = gameSave.Id,
            UserId = gameSave.UserId,
            SaveName = gameSave.SaveName,
            PlayerCharacterId = gameSave.PlayerCharacterId,
            CurrentStoryNodeId = gameSave.CurrentStoryNodeId,
            LastUpdate = gameSave.LastUpdate
        };
    }





    /// <summary>
    /// Additional methods for story navigation and choice. 
    /// </summary>
    /// <param name="nodeId">The ID of the story node</param>
    /// <returns></returns>
    public async Task<StoryNodeDto?> GetNodeAsync(int nodeId)
    {
        // Get the story node by its ID
        var storyNode = await _uow.StoryNodeRepository.GetById(nodeId);
        // If not found, return null
        if (storyNode == null)
        {
            return null;
        }
        //Get the dialogues and choices
        var dialogues = await _uow.StoryNodeRepository.GetAllDialoguesOfStoryNode(storyNode.Id);
        var choices = await _uow.StoryNodeRepository.GetAllChoicesOfStoryNode(storyNode.Id);

        //Returns the story node DTO
        return new StoryNodeDto
        {
            Id = storyNode.Id,
            Title = storyNode.Title,
            Description = storyNode.Description,
            BackgroundUrl = storyNode.BackgroundUrl,
            BackgroundMusicUrl = storyNode.BackgroundMusicUrl,
            AmbientSoundUrl = storyNode.AmbientSoundUrl,

            //Order dialogues
            Dialogues = dialogues
                .OrderBy(d => d.Order)
                .Select(d => new DialogueDto
                {
                    Id = d.Id,
                    Text = d.Text,
                    CharacterId = d.CharacterId,
                    Order = d.Order
                }).ToList(),
            //Map choices
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
    /// <param name="currentNodeId">The current story node ID</param>
    /// <param name="choiceId">The choice ID</param>
    /// <returns>The next story node ID, or null if no next node</returns>
    /// </summary>
    public async Task<int?> ApplyChoiceAsync(int currentNodeId, int choiceId)
    {
        // Validate that choice belongs to currentNodeId
        var choice = await _uow.ChoiceRepository.GetById(choiceId);
        //If choice is not found, return null
        if (choice == null)
        {
            return null;
        }
        //If the currenct choice does not belong on the current story node throw an error.
        if (choice.StoryNodeId != currentNodeId)
        {
            await _entityLogger.LogAsync(
                "ChoiceDoesNotBelongToCurrentNode",
                new
                {
                    ChoiceId = choiceId,
                    CurrentNodeId = currentNodeId,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.GamePlay.StoryHandling,
                "ChoiceDoesNotBelongToCurrentNodeLog");
            throw new InvalidOperationException("Choice does not belong to current node.");
        }

        // Return next node ID (can be null for end)
        return choice.NextStoryNodeId;
    }
}

