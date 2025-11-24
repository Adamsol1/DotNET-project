namespace backend.Application.Dtos;

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

public sealed class CreateStoryNodeDto {
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundUrl { get; set; } = string.Empty;
}

public sealed class UpdateStoryNodeDto {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackgroundUrl { get; set; } = string.Empty;
}