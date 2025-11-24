namespace backend.Domain.Models;

/// <summary>
/// Represents a piece of dialogue in a story node, optionally associated with a character.
/// </summary>
public class Dialogue
{
    /// <summary>
    /// The unique identifier for the dialogue.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The identifier of the story node this dialogue belongs to.
    /// </summary>
    public int StoryNodeId { get; set; }

    /// <summary>
    /// The story node this dialogue belongs to.
    /// test string
    /// </summary>
    public StoryNode StoryNode { get; set; } = null!;
    
    /// <summary>
    /// The order of the dialogue within the story node.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The identifier of the character speaking this dialogue, if any.
    /// </summary>
    public int CharacterId { get; set; }
    /// <summary>
    /// The character speaking this dialogue, if any.
    /// </summary>
    public Character? Character { get; set; }

    /// <summary>
    /// The text of the dialogue.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}