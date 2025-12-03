namespace backend.Application.Dtos.Story;

/// <summary>
/// DTO used for dialouges in the story associated with a character and a node.  
/// </summary>
public sealed class DialogueDto
{
    public int Id { get; set; }
    public int StoryNodeId { get; set; }
    public int Order { get; set; }
    public int CharacterId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CharacterName { get; set; }
    public string? CharacterImageUrl { get; set; }
}

/// <summary>
/// DTO used for creating a dialogue for the story. 
/// </summary>
public sealed class CreateDialogueDto
{
    public int StoryNodeId { get; set; }
    public int Order { get; set; }
    public int CharacterId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CharacterName { get; set; }
    public string? CharacterImageUrl { get; set; }
}

/// <summary>
/// DTO used for updating existing dialogue in story. 
/// </summary>
public sealed class UpdateDialogueDto
{
    public int Id { get; set; }
    public int StoryNodeId { get; set; }
    public int Order { get; set; }
    public int CharacterId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CharacterName { get; set; }
    public string? CharacterImageUrl { get; set; }
}

