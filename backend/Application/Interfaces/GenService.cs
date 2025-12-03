using backend.Application.Dtos.Game;
using backend.Application.Dtos.Story;
using backend.Domain.Models;
using backend.Infrastructure.Repositories.Base;
using backend.Infrastructure.Logging;

namespace backend.Application.Interfaces;

public class GenService : IGenService
{
    private readonly IUnitOfWork _uow;
    private readonly IEntityFileLogger _entityLogger;

    public GenService(IUnitOfWork uow, IEntityFileLogger entityLogger)
    {
        _uow = uow;
        _entityLogger = entityLogger;
    }

    #region Execution methods.

    /// <summary>
    /// A generic service that is used in the services that inherit from GenService.
    /// The service contains methods that handle common operations such as transactions.
    /// Excutes an operation and returns a result.
    /// Used for operations where we can pass in an an function method to execute.
    // and delegate the work to the caller.
    /// <typeparam name="T">Generic type parameter</typeparam>

    /// </summary>
    public async Task<T> Execute<T>(Func<Task<T>> request)
    {
        // start a transaction to avoid aloways doing this in each crud service function.
        await _uow.BeginAsync();

        try
        {
            // execute the request.
            var result = await request();
            // save and commit the transaction.
            await _uow.SaveAsync();
            // commit the transaction.
            await _uow.CommitAsync();
            return result;

        }
        catch (Exception ex)
        {
            // if the try fails, we rollback the transaction.
            await _uow.RollBackAsync();
            // throw the exception to the caller.
            // since we dont know what kind of exception it is, we just throw it
            // and let the caller handle it.
            await _entityLogger.LogAsync(
                "GenService: unexpected error occurred",
                new
                {
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.SystemLevel.Errors
            );
            throw;
        }
    }


    /// <summary>
    /// Executes an operation without return value
    /// This counterpart method does the same execution as above, but for void. 
    /// </summary>

    public async Task Execute(Func<Task> request)
    {
        await _uow.BeginAsync();
        try
        {
            await request();
            await _uow.SaveAsync();
            await _uow.CommitAsync();
        }
        catch (Exception)
        {
            await _uow.RollBackAsync();
            throw;
        }
    }

    #endregion

    #region DTO mapper methods.

    //

    /// <summary>
    ///  map storyNode, dialogue and choice arrays to a StoryNodeDto.
    ///  This is made for avoiding repeating this in each service that might use it.
    /// </summary>
    /// <param name="storyNode">The node</param>
    /// <param name="dialogues">The dialogues of the node</param>
    /// <param name="choices">The choices of the node</param>
    public StoryNodeDto MapStoryNode(StoryNode storyNode, IEnumerable<Dialogue> dialogues, IEnumerable<Choice> choices)
    {

        return new StoryNodeDto
        {
            Id = storyNode.Id,
            Title = storyNode.Title,
            Description = storyNode.Description,
            BackgroundUrl = storyNode.BackgroundUrl ?? string.Empty,
            BackgroundMusicUrl = storyNode.BackgroundMusicUrl,
            AmbientSoundUrl = storyNode.AmbientSoundUrl,

            // dialogues and choices where we loop through and map those into their counterpart dtos.
            Dialogues = dialogues.OrderBy(d => d.Order)
            .Select(MapDialogue)
            .ToList(),

            Choices = choices.Select(MapChoice)
            .ToList()
        };
    }


    /// <summary>
    /// Maps a choice to a ChoiceDto.
    /// </summary>
    /// <param name="choice">The choice</param>
    /// <returns>Choice DTO</returns>
    public ChoiceDto MapChoice(Choice choice)
    {
        return new ChoiceDto
        {
            Id = choice.Id,
            Text = choice.Text,
            StoryNodeId = choice.StoryNodeId,
            NextStoryNodeId = choice.NextStoryNodeId,
            AudioUrl = choice.AudioUrl,
            HealthEffect = choice.HealthEffect
        };
    }


    /// <summary>
    /// Maps a dialogue to a DialogueDto.
    /// </summary>
    /// <param name="dialogue">The dialogue</param>
    /// <returns>Dialogue DTO</returns>
    public DialogueDto MapDialogue(Dialogue dialogue)
    {
        return new DialogueDto
        {
            Id = dialogue.Id,
            Text = dialogue.Text,
            CharacterId = dialogue.CharacterId,
            StoryNodeId = dialogue.StoryNodeId,
            Order = dialogue.Order,
            CharacterName = dialogue.Character?.Name ?? string.Empty,
            CharacterImageUrl = dialogue.Character?.ImageUrl ?? string.Empty
        };
    }


    /// <summary>
    /// Maps a character and its dialogues to a CharacterDto.
    /// </summary>
    /// <param name="character">The given character</param>
    /// <param name="dialogues">The dialogues of the character</param>
    /// <returns>Character DTO</returns>
    public CharacterDto MapCharacter(Character character, IEnumerable<Dialogue> dialogues)
    {
        return new CharacterDto
        {
            Id = character.Id,
            Name = character.Name,
            Description = character.Description ?? string.Empty,
            ImageUrl = character.ImageUrl ?? string.Empty,
            Dialogues = dialogues.Select(MapDialogue).ToList()
        };
    }

    /// <summary>
    /// Maps a GameSave to a GameSaveDto.
    /// </summary>
    /// <param name="gameSave">The game save</param>
    /// <returns>Gamesave DTO</returns>
    public GameSaveDto MapGameSave(GameSave gameSave)
    {
        return new GameSaveDto
        {
            Id = gameSave.Id,
            UserId = gameSave.UserId,
            SaveName = gameSave.SaveName,
            PlayerCharacterId = gameSave.PlayerCharacterId,
            CurrentStoryNodeId = gameSave.CurrentStoryNodeId,
            LastUpdate = gameSave.LastUpdate,
            Health = gameSave.Health
        };
    }

    #endregion

    #region Validation methods.

    /**
    / The validation methods are used to validate data and comes with checks prebuilt, this is to create a common validation handler.
    / </summary>
    / <typeparam name="T">Generic type</typeparam>
    / <param name="id">The id</param>
    / <returns>The entity</returns>
   */



    /// <summary>
    /// function that checks if an entity exists by id.
    /// </summary>
    /// <typeparam name="T">Generic type parameter</typeparam>
    /// <param name="id">The id of the entity</param>
    public async Task<T> ValidateEntityExists<T>(int id) where T : class
    {
        // the getRepository method fetches the repository we are looking for.
        var entity = await _uow.GetRepository<T>().GetById(id);
        if (entity != null)
        {
            return entity;
        }
        else
        {
            var entityName = typeof(T).Name;
            throw new KeyNotFoundException($"{entityName} with id {id} not found");
        }

    }


    /// <summary>
    /// Checks if a choice belongs to a story node.
    /// </summary>
    /// <param name="choiceId">The id of the choice</param>
    /// <param name="nodeId">The id of the node</param>
    /// <returns>A boolean value indicating if a choice belongs to the node or not</returns>
    public async Task<bool> CheckChoiceInNode(int choiceId, int nodeId)
    {
        // get the choice from the repository.
        var choice = await ValidateEntityExists<Choice>(choiceId);

        return choice.StoryNodeId == nodeId;

    }

    /// <summary>
    /// Checks if a story node exists.
    /// Will return a boolean value indicating if it exists or not.
    /// </summary>
    /// <param name="nodeId">The id of the node</param>
    /// <returns>A boolean value indicating if the story node exists or not</returns>
    public async Task<bool> CheckStoryNodeExists(int nodeId)
    {
        var storyNode = await _uow.StoryNodeRepository.GetById(nodeId);

        if (storyNode == null)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region common methods.



    #endregion
}