using backend.Domain.Models;

namespace backend.Application.Interfaces.Repositories;


public interface IChoiceRepository : IGenericRepository<Choice>
{
    /// <summary>
    /// Get StoryNode id this choice belongs to
    /// </summary>
    Task<int> GetStoryNodeId(int id);

    /// <summary>
    /// Get StoryNode this choice belongs to
    /// </summary>

    Task<StoryNode?> GetStoryNode(int id);

    /// <summary>
    /// Get id of story node this choice leads to
    /// </summary>

    Task<int> GetNextStoryNodeId(int id);

    /// <summary>
    /// Get the story node this choice leads to'
    /// </summary>
    Task<StoryNode?> GetNextStoryNode(int id);
    
    /// <summary>
    /// Get text given in this choice
    /// </summary>
    Task<string> GetChoiceText(int id);

    /// <summary>
    /// Get all choices for a specific story node
    /// </summary>
    Task<IEnumerable<Choice>> GetAllByStoryNodeId(int storyNodeId);
}