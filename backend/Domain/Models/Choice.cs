

using System.ComponentModel.DataAnnotations;
namespace backend.Domain.Models;

/// <summary>
/// Represents a choice in a story node that leads to another story node.
/// </summary>
public class Choice
{
    /// <summary>
    /// The unique identifier for the choice.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// The identifier of the story node this choice belongs to.
    /// </summary>
    public int StoryNodeId { get; set; }

    /// <summary>
    /// The story node this choice belongs to.
    /// </summary>
    public StoryNode StoryNode { get; set; } = null!;
    
    /// <summary>
    /// The identifier of the next story node this choice leads to.
    /// </summary>
    public int NextStoryNodeId { get; set; }
    public StoryNode? NextStoryNode { get; set; }

    /// <summary>
    /// The text displayed for this choice.
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// The health effect when this choice is selected.
    /// Positive values add health, negative values deal damage.
    /// Null if this choice has no health effect.
    /// </summary>
    public int? HealthEffect { get; set; }

    /// <summary>
    /// The URL to an audio file to play when this choice is selected.
    /// Null or empty if no audio should be played.
    /// </summary>
    [StringLength(500)]
    public string? AudioUrl { get; set; }

}