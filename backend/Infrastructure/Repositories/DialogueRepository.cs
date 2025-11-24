using backend.Domain.Models;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories;


public class DialogueRepository : GenericRepository<Dialogue>, IDialogueRepository
{

    private readonly AppDbContext _db;
    public DialogueRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }
  
    /// <summary>
    /// Get a dialogue by ID with its Character eagerly loaded
    /// </summary>
    public async Task<Dialogue?> GetByIdWithCharacter(int id)
    {
        return await _db.Dialogues
            .Include(d => d.Character)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <summary>
    /// Get all dialogues for a story node with Characters eagerly loaded
    /// </summary>
    public async Task<IEnumerable<Dialogue>> GetAllByStoryNodeWithCharacter(int storyNodeId)
    {
        return await _db.Dialogues
            .Include(d => d.Character)
            .Where(d => d.StoryNodeId == storyNodeId)
            .OrderBy(d => d.Order)
            .ToListAsync();
    }

    /// <summary>
    /// Get the id of the story node where dialogue is shown
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<int> GetStoryNodeId(int id)
    {
        /// Query to get StoryNode id this dialogue belongs to
        var storyNodeId = _db.Dialogues
                    .Where(Dialogues => Dialogues.Id == id)
                    .Select(Dialogues => Dialogues.StoryNodeId)
                    .SingleOrDefaultAsync();

        return await storyNodeId;
    }

        /// <summary>
        /// Get the StoryNode this dialogue belongs to
        /// The method expects either one or zero results because the storynode ID is unique.
        /// </summary>

        public async Task<StoryNode?> GetStoryNode(int id)
        {
            /// Query to get StoryNode this choice belongs to
            var storyNode = _db.Dialogues
                        .Where(Dialogues => Dialogues.Id == id)
                        .Select(Dialogues => Dialogues.StoryNode)
                        .SingleOrDefaultAsync();

            return await storyNode;
        }

    /// <summary>
    /// Get the order the dialogues is shown in current story node
    /// The method expects either one or zero results because the dialogue should only be used once per story node.
    /// </summary>

    public async Task<int> GetDialogueOrder(int id)
    {
        /// Query to get the order the dialogue is shown in current story node
        var dialogueOrder = _db.Dialogues
                    .Where(Dialogues => Dialogues.Id == id)
                    .Select(Dialogues => Dialogues.Order)
                    .SingleOrDefaultAsync();

        return await dialogueOrder;
    }


    /// <summary>
    /// Get the character id of the character speaking the dialogue
    /// </summary>

    public async Task<int> GetCharacterId(int id)
    {
        /// Query to get the character id of the character speaking the dialogue
        var characterId = _db.Dialogues
                    .Where(Dialogues => Dialogues.Id == id)
                    .Select(Dialogues => Dialogues.CharacterId)
                    .SingleOrDefaultAsync();

        return await characterId;
    }

    /// <summary>
    /// Get the character speaking the dialogue
    /// The method expects either one or zero results because not all dialogues have a character. For example, narration and sound effects.
    /// </summary>

    public async Task<Character?> GetCharacter(int id)
    {
        /// Query to get the character speaking the dialogue
        var character = _db.Dialogues
                    .Where(Dialogues => Dialogues.Id == id)
                    .Select(Dialogues => Dialogues.Character)
                    .SingleOrDefaultAsync();

        return await character;
    }

    /// <summary>
    /// Get the dialogues text
    /// The method expects either one or zero results because the dialogue ID is unique.
    /// </summary>
    
    
    public async Task<string> GetDialogueText(int id)
    {
        /// Query to get the dialogues text
        var dialogueText = _db.Dialogues
                    .Where(Dialogues => Dialogues.Id == id)
                    .Select(Dialogues => Dialogues.Text)
                    .SingleOrDefaultAsync();

        return await dialogueText;
    }
}