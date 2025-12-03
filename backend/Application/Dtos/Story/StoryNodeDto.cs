namespace backend.Application.Dtos.Story;

/// <summary>
/// DTO used for story nodes in the story. Includes all dialogue, choices, and other attributes.
/// </summary>
public sealed class StoryNodeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundUrl { get; set; } = string.Empty;
    public string? BackgroundMusicUrl { get; set; }
    public string? AmbientSoundUrl { get; set; }
    public List<DialogueDto> Dialogues { get; set; } = new List<DialogueDto>();
    public List<ChoiceDto> Choices { get; set; } = new List<ChoiceDto>();
}
/// <summary>
/// Dto used for creating a new story node
/// </summary>
public sealed class CreateStoryNodeDto {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundUrl { get; set; } = string.Empty;
}

/// <summary>
/// Dto used for updating existing story node
/// </summary>
public sealed class UpdateStoryNodeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundUrl { get; set; } = string.Empty;
}