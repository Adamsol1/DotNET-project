using backend.Domain.Models;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Logging;
using System;

namespace backend.Infrastructure.Repositories;


/// <summary>
/// Repository for managing StoryNode entities in the database.
/// </summary>
public class StoryNodeRepository : GenericRepository<StoryNode>, IStoryNodeRepository
{
    private readonly AppDbContext _db;
    private readonly IEntityFileLogger _entityLogger;
    
    public StoryNodeRepository(AppDbContext db, IEntityFileLogger entityLogger) : base(db, entityLogger)
    {
        _db = db;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Get the story node title with ID
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>
    public async Task<string?> GetStoryNodeTitleById(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(id));
            }
            
            var title = await _db.StoryNodes
                        .Where(StoryNodes => StoryNodes.Id == id)
                        .Select(StoryNodes => StoryNodes.Title)
                        .SingleOrDefaultAsync();

            return title;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetStoryNodeTitleByIdError",
                new { StoryNodeId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get the story node with given title
    /// </summary>

    public async Task<StoryNode?> GetStoryNodeByTitle(string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("StoryNode title cannot be null or empty", nameof(title));
            }
            
            var storyNode = await _db.StoryNodes
                        .Where(StoryNodes => StoryNodes.Title == title)
                        .FirstOrDefaultAsync();

            return storyNode;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetStoryNodeByTitleError",
                new { Title = title, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get description of the story node with given ID
    /// </summary>
    public async Task<string?> GetStoryNodeDescription(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(id));
            }
            
            var description = await _db.StoryNodes
                        .Where(StoryNodes => StoryNodes.Id == id)
                        .Select(StoryNodes => StoryNodes.Description)
                        .SingleOrDefaultAsync();

            return description;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetStoryNodeDescriptionError",
                new { StoryNodeId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get the URL of the StoryNode background given by ID
    /// </summary>

    public async Task<string?> GetStoryNodeBackgroundUrl(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(id));
            }
            
            var background = await _db.StoryNodes
                        .Where(StoryNodes => StoryNodes.Id == id)
                        .Select(StoryNodes => StoryNodes.BackgroundUrl)
                        .SingleOrDefaultAsync();

            return background;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetStoryNodeBackgroundUrlError",
                new { StoryNodeId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get all dialougues associated with a story node given by ID
    /// </summary>

    public async Task<IEnumerable<Dialogue>> GetAllDialoguesOfStoryNode(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(id));
            }
            
            var dialogues = await _db.Dialogues
                        .Where(Dialogues => Dialogues.StoryNodeId == id)
                        .ToListAsync();

            return dialogues;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllDialoguesOfStoryNodeError",
                new { StoryNodeId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get all choices associated with a story node given by ID
    /// </summary>

    public async Task<IEnumerable<Choice>> GetAllChoicesOfStoryNode(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(id));
            }
            
            var choices = await _db.Choices
                        .Where(Choices => Choices.StoryNodeId == id)
                        .ToListAsync();

            return choices;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllChoicesOfStoryNodeError",
                new { StoryNodeId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    
    //TODO: Can be removed?
    // Ahmed, 11.10 Added GetAllCharactersOfStoryNode method
    public async Task<IEnumerable<Character>> GetAllCharactersOfStoryNode(int id)
    {

        // Query to get charachters in a storynode by Id
        // Characters are found through dialogues in the story node
        // We might need to delink characters from dialogues in the future?
        // so we dont need to go through the dialogues to get the characters
        // but for now, I will just do it like this to avoid changing alot and touch AppContext
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(id));
            }
            
            var characters = await _db.Dialogues
                    .Where(d => d.StoryNodeId == id && d.CharacterId != null)
                    .Select(d => d.Character)
                    .Where(c => c != null)
                    .Select(c => c!)
                    .Distinct()
                    .ToListAsync();

            return characters;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllCharactersOfStoryNodeError",
                new { StoryNodeId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

}