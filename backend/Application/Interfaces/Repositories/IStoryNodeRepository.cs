

using backend.Domain.Models;

namespace backend.Application.Interfaces.Repositories;

public interface IStoryNodeRepository : IGenericRepository<StoryNode>
{
    /// <summary>
    /// Get the story node title with ID
    /// </summary>

    Task<String> GetStoryNodeTitleById(int id);

    /// <summary>
    /// Get the story node with given title
    /// </summary>
    Task<StoryNode?> GetStoryNodeByTitle(string title);

    /// <summary>
    /// Get description of the story node with given ID
    /// </summary>
    Task<String> GetStoryNodeDescription(int id);

    /// <summary>
    /// Get the URL of the StoryNode background given by ID
    /// </summary>

    Task<String> GetStoryNodeBackgroundUrl(int id);

    /// <summary>
    /// Get all dialougues associated with a story node given by ID
    /// </summary>
    Task<IEnumerable<Dialogue>> GetAllDialoguesOfStoryNode(int id);
    
    /// <summary>
    /// Get all choices associated with a story node given by ID
    /// </summary>
    Task<IEnumerable<Choice>> GetAllChoicesOfStoryNode(int id);
    
    // Added this method to get all characters in a story node
    Task<IEnumerable<Character>> GetAllCharactersOfStoryNode(int id);
}