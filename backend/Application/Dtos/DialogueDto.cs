namespace backend.Application.Dtos;

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

public sealed class CreateDialogueDto
{
    public int StoryNodeId { get; set; }
    public int Order { get; set; }
    public int CharacterId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CharacterName { get; set; }
    public string? CharacterImageUrl { get; set; }
}

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

