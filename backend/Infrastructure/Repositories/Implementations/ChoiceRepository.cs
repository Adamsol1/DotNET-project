using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Logging;
using System;

namespace backend.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository for managing Choice entities in the database.
/// </summary>
public class ChoiceRepository : GenericRepository<Choice>, IChoiceRepository
{

    private readonly AppDbContext _db;
    private readonly IEntityFileLogger _entityLogger;
    
    public ChoiceRepository(AppDbContext db, IEntityFileLogger entityLogger) : base(db, entityLogger)
    {
        _db = db;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Get the StoryNode id this choice belongs to
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<int> GetStoryNodeId(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Choice ID must be greater than zero", nameof(id));
            }
            
            var storyNodeId = await _db.Choices
                        .Where(Choices => Choices.Id == id)
                        .Select(Choices => Choices.StoryNodeId)
                        .SingleOrDefaultAsync();

            return storyNodeId;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetStoryNodeIdError",
                new { ChoiceId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.System);
            throw;
        }
    }

    /// <summary>
    /// Get the StoryNode this choice belongs to
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<StoryNode?> GetStoryNode(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Choice ID must be greater than zero", nameof(id));
            }
            
            var storyNode = await _db.Choices
                        .Where(Choices => Choices.Id == id)
                        .Select(Choices => Choices.StoryNode)
                        .SingleOrDefaultAsync();

            return storyNode;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetStoryNodeError",
                new { ChoiceId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.System);
            throw;
        }
    }

    /// <summary>
    /// Get the next story node this choice leads to
    /// The method expects either one or zero results because the next storynode ID is unique.
    /// </summary>

    public async Task<StoryNode?> GetNextStoryNode(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Choice ID must be greater than zero", nameof(id));
            }
            
            var nextStoryNode = await _db.Choices
                        .Where(Choices => Choices.Id == id)
                        .Select(Choices => Choices.NextStoryNode)
                        .SingleOrDefaultAsync();

            return nextStoryNode;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetNextStoryNodeError",
                new { ChoiceId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.System);
            throw;
        }
    }

    /// <summary>
    /// Get the id of the next story node this choice leads to
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<int> GetNextStoryNodeId(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Choice ID must be greater than zero", nameof(id));
            }
            
            var nextStoryNodeId = await _db.Choices
                        .Where(Choices => Choices.Id == id)
                        .Select(Choices => Choices.NextStoryNodeId)
                        .SingleOrDefaultAsync();

            return nextStoryNodeId;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetNextStoryNodeIdError",
                new { ChoiceId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.System);
            throw;
        }
    }

    /// <summary>
    /// Get the text given in this choice'
    /// </summary>


    public async Task<string?> GetChoiceText(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Choice ID must be greater than zero", nameof(id));
            }
            
            var choiceText = await _db.Choices
                        .Where(Choices => Choices.Id == id)
                        .Select(Choices => Choices.Text)
                        .SingleOrDefaultAsync();

            return choiceText;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetChoiceTextError",
                new { ChoiceId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.System);
            throw;
        }
    }

    public async Task<IEnumerable<Choice>> GetAllByStoryNodeId(int storyNodeId)
    {
        try
        {
            if (storyNodeId <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(storyNodeId));
            }
            
            return await _db.Choices
                .Where(c => c.StoryNodeId == storyNodeId)
                .ToListAsync();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllByStoryNodeIdError",
                new { StoryNodeId = storyNodeId, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.System);
            throw;
        }
    }

}