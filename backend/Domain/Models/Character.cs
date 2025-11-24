namespace backend.Domain.Models;

/// <summary>
/// Represents a character in the story.
/// </summary>
public class Character
{
    /// <summary>
    /// The unique identifier for the character.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// A brief description of the character.
    /// </summary>
    public string? Description { get; set; } = string.Empty;
    /// <summary>
    /// URL to an image representing the character.
    /// </summary>
    public string? ImageUrl { get; set; } = string.Empty;
    /// <summary>
    /// Dialogues associated with the character.
    /// </summary>
    public ICollection<Dialogue> Dialogues { get; set; } = new List<Dialogue>();
}