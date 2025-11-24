using backend.Domain.Models;
using backend.Application.Interfaces.Repositories;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace backend.Infrastructure.Repositories;


public class ChoiceRepository : GenericRepository<Choice>, IChoiceRepository
{

    private readonly AppDbContext _db;
    public ChoiceRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    /// <summary>
    /// Get the StoryNode id this choice belongs to
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<int> GetStoryNodeId(int id)
    {
        /// Query to get StoryNode id this choice belongs to
        var storyNodeId = _db.Choices
                    .Where(Choices => Choices.Id == id)
                    .Select(Choices => Choices.StoryNodeId)
                    .SingleOrDefaultAsync();

        return await storyNodeId;
    }

    /// <summary>
    /// Get the StoryNode this choice belongs to
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<StoryNode?> GetStoryNode(int id)
    {
        /// Query to get StoryNode this choice belongs to
        var storyNode = _db.Choices
                    .Where(Choices => Choices.Id == id)
                    .Select(Choices => Choices.StoryNode)
                    .SingleOrDefaultAsync();

        return await storyNode;
    }

    /// <summary>
    /// Get the next story node this choice leads to
    /// The method expects either one or zero results because the next storynode ID is unique.
    /// </summary>

    public async Task<StoryNode?> GetNextStoryNode(int id)
    {
        /// Query to get id of story node this choice leads to
        var nextStoryNodeId = _db.Choices
                    .Where(Choices => Choices.Id == id)
                    .Select(Choices => Choices.NextStoryNode)
                    .SingleOrDefaultAsync();

        return await nextStoryNodeId;
    }

    /// <summary>
    /// Get the id of the next story node this choice leads to
    /// The method expects either one or zero results because the storynode ID is unique.
    /// </summary>

    public async Task<int> GetNextStoryNodeId(int id)
    {
        /// Query to get id of story node this choice leads to
        var nextStoryNodeId = _db.Choices
                    .Where(Choices => Choices.Id == id)
                    .Select(Choices => Choices.NextStoryNodeId)
                    .SingleOrDefaultAsync();

        return await nextStoryNodeId;
    }

    /// <summary>
    /// Get the text given in this choice'
    /// </summary>


    public async Task<string> GetChoiceText(int id)
    {
        /// Query to get text given in this choice
        var choiceText = _db.Choices
                    .Where(Choices => Choices.Id == id)
                    .Select(Choices => Choices.Text)
                    .SingleOrDefaultAsync();

        return await choiceText;
    }

    public async Task<IEnumerable<Choice>> GetAllByStoryNodeId(int storyNodeId)
    {
        return await _db.Choices
            .Where(c => c.StoryNodeId == storyNodeId)
            .ToListAsync();
    }

}