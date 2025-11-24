using backend.Domain.Models;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Repositories;

public class StoryNodeRepository : GenericRepository<StoryNode>, IStoryNodeRepository
{
    private readonly AppDbContext _db;
    public StoryNodeRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    /// <summary>
    /// Get the story node title with ID
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<String> GetStoryNodeTitleById(int id)
    {
        /// Query to get title of story node with given ID
        var title = _db.StoryNodes
                    .Where(StoryNodes => StoryNodes.Id == id)
                    .Select(StoryNodes => StoryNodes.Title)
                    .SingleOrDefaultAsync();

        return await title;
    }

    /// <summary>
    /// Get the story node with given title
    /// </summary>

    public async Task<StoryNode?> GetStoryNodeByTitle(string title)
    {
        /// Query to get story node with given title
        var storyNode = _db.StoryNodes
                    .Where(StoryNodes => StoryNodes.Title == title)
                    .FirstOrDefaultAsync();

        return await storyNode;
    }

    /// <summary>
    /// Get description of the story node with given ID
    /// </summary>
    public async Task<String> GetStoryNodeDescription(int id)
    {
        /// Query to get description of story node with given ID
        var description = _db.StoryNodes
                    .Where(StoryNodes => StoryNodes.Id == id)
                    .Select(StoryNodes => StoryNodes.Description)
                    .SingleOrDefaultAsync();

        return await description;
    }

    /// <summary>
    /// Get the URL of the StoryNode background given by ID
    /// </summary>

    public async Task<String> GetStoryNodeBackgroundUrl(int id)
    {
        /// Query to get URL of StoryNode background given by ID
        var background = _db.StoryNodes
                    .Where(StoryNodes => StoryNodes.Id == id)
                    .Select(StoryNodes => StoryNodes.BackgroundUrl)
                    .SingleOrDefaultAsync();

        return await background;
    }

    /// <summary>
    /// Get all dialougues associated with a story node given by ID
    /// </summary>

    public async Task<IEnumerable<Dialogue>> GetAllDialoguesOfStoryNode(int id)
    {
        /// Query to get all dialogues associated with a story node given by ID in list
        var dialogues = _db.Dialogues
                    .Where(Dialogues => Dialogues.StoryNodeId == id)
                    .ToListAsync();

        return await dialogues;
    }

    /// <summary>
    /// Get all choices associated with a story node given by ID
    /// </summary>
    
    public async Task<IEnumerable<Choice>> GetAllChoicesOfStoryNode(int id)
    {
        /// Query to get all choices associated with a story node given by ID in list
        var choices = _db.Choices
                    .Where(Choices => Choices.StoryNodeId == id)
                    .ToListAsync();

        return await choices;
    }

    // Ahmed, 11.10 Added GetAllCharactersOfStoryNode method
    public async Task<IEnumerable<Character>> GetAllCharactersOfStoryNode(int id)
    {
        // Query to get charachters in a storynode by Id
        // Characters are found through dialogues in the story node
        // We might need to delink characters from dialogues in the future?
        // so we dont need to go through the dialogues to get the characters
        // but for now, I will just do it like this to avoid changing alot and touch AppContext
        var characters = _db.Dialogues
                    .Where(d => d.StoryNodeId == id && d.CharacterId != null)
                    .Select(d => d.Character)
                    .Where(c => c != null)
                    .Distinct()
                    .ToListAsync();
        return await characters;
    }

}