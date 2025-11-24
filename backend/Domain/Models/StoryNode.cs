using System.ComponentModel.DataAnnotations;
namespace backend.Domain.Models;
/// <summary>
/// Represents a node in the story, containing dialogues and choices.
/// </summary>
public class StoryNode
{
    /// <summary>
    /// The unique identifier for the story node.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the story node.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// A brief description of the story node.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// URL to a background image for the story node.
    /// </summary>
    public string? BackgroundUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// URL to background music for this story node.
    /// </summary>
    [StringLength(500)]
    public string? BackgroundMusicUrl { get; set; }
    
    /// <summary>
    /// URL to ambient/effect sound for this story node (e.g., alarm, wind, rain).
    /// </summary>
    [StringLength(500)]
    public string? AmbientSoundUrl { get; set; }

    /// <summary>
    /// Dialogues associated with the story node.
    /// </summary>
    public ICollection<Dialogue> Dialogues { get; set; } = new List<Dialogue>();

    /// <summary>
    /// Choices available at the story node.
    /// </summary>
    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}
    
