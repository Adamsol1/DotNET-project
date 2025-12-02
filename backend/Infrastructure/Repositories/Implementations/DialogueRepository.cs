using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Logging;
using System;

namespace backend.Infrastructure.Repositories.Implementations;

/// <summary>
/// Repository for managing Dialogue entities in the database.
/// </summary>
public class DialogueRepository : GenericRepository<Dialogue>, IDialogueRepository
{

    private readonly AppDbContext _db;
    private readonly IEntityFileLogger _entityLogger;
    
    public DialogueRepository(AppDbContext db, IEntityFileLogger entityLogger) : base(db, entityLogger)
    {
        _db = db;
        _entityLogger = entityLogger;
    }
  
    /// <summary>
    /// Get a dialogue by ID with its Character eagerly loaded
    /// </summary>
    public async Task<Dialogue?> GetByIdWithCharacter(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
            }
            
            return await _db.Dialogues
                .Include(d => d.Character)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetByIdWithCharacterError",
                new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get all dialogues for a story node with Characters eagerly loaded
    /// </summary>
    public async Task<IEnumerable<Dialogue>> GetAllByStoryNodeWithCharacter(int storyNodeId)
    {
        try
        {
            if (storyNodeId <= 0)
            {
                throw new ArgumentException("StoryNode ID must be greater than zero", nameof(storyNodeId));
            }
            
            return await _db.Dialogues
                .Include(d => d.Character)
                .Where(d => d.StoryNodeId == storyNodeId)
                .OrderBy(d => d.Order)
                .ToListAsync();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetAllByStoryNodeWithCharacterError",
                new { StoryNodeId = storyNodeId, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get the id of the story node where dialogue is shown
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<int> GetStoryNodeId(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
            }
            
            var storyNodeId = await _db.Dialogues
                        .Where(Dialogues => Dialogues.Id == id)
                        .Select(Dialogues => Dialogues.StoryNodeId)
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
                new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

        /// <summary>
        /// Get the StoryNode this dialogue belongs to
        /// The method expects either one or zero results because the storynode ID is unique.
        /// </summary>

        public async Task<StoryNode?> GetStoryNode(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
                }
                
                var storyNode = await _db.Dialogues
                            .Where(Dialogues => Dialogues.Id == id)
                            .Select(Dialogues => Dialogues.StoryNode)
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
                    new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                    LogCategories.SystemLevel.Database);
                throw;
            }
        }

    /// <summary>
    /// Get the order the dialogues is shown in current story node
    /// The method expects either one or zero results because the dialogue should only be used once per story node.
    /// </summary>

    public async Task<int> GetDialogueOrder(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
            }
            
            var dialogueOrder = await _db.Dialogues
                        .Where(Dialogues => Dialogues.Id == id)
                        .Select(Dialogues => Dialogues.Order)
                        .SingleOrDefaultAsync();

            return dialogueOrder;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetDialogueOrderError",
                new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }


    /// <summary>
    /// Get the character id of the character speaking the dialogue
    /// </summary>

    public async Task<int> GetCharacterId(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
            }
            
            var characterId = await _db.Dialogues
                        .Where(Dialogues => Dialogues.Id == id)
                        .Select(Dialogues => Dialogues.CharacterId)
                        .SingleOrDefaultAsync();

            return characterId;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetCharacterIdError",
                new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get the character speaking the dialogue
    /// The method expects either one or zero results because not all dialogues have a character. For example, narration and sound effects.
    /// </summary>

    public async Task<Character?> GetCharacter(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
            }
            
            var character = await _db.Dialogues
                        .Where(Dialogues => Dialogues.Id == id)
                        .Select(Dialogues => Dialogues.Character)
                        .SingleOrDefaultAsync();

            return character;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetCharacterError",
                new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }

    /// <summary>
    /// Get the dialogues text
    /// The method expects either one or zero results because the dialogue ID is unique.
    /// </summary>
    
    
    public async Task<string?> GetDialogueText(int id)
    {
        try
        {
            if (id <= 0)
            {
                throw new ArgumentException("Dialogue ID must be greater than zero", nameof(id));
            }
            
            var dialogueText = await _db.Dialogues
                        .Where(Dialogues => Dialogues.Id == id)
                        .Select(Dialogues => Dialogues.Text)
                        .SingleOrDefaultAsync();

            return dialogueText;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "GetDialogueTextError",
                new { DialogueId = id, Reason = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.SystemLevel.Database);
            throw;
        }
    }
}