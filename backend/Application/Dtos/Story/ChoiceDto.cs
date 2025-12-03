namespace backend.Application.Dtos.Story;

/// <summary>
/// DTO for a choice on a story node in the story.
/// This does also include possible audio cues and health effects.
/// </summary>
public sealed class ChoiceDto
{
    public int Id { get; set; }
    public int StoryNodeId { get; set; }
    public int NextStoryNodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int? HealthEffect { get; set; }
}

/// <summary>
/// DTO used for creating a new choice with all attributes. 
/// </summary>
public sealed class CreateChoiceDto
{
    public int StoryNodeId { get; set; }
    public int NextStoryNodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int? HealthEffect { get; set; }
}

/// <summary>
/// DTO used for updating an existing choice with all attributes
/// </summary>
public sealed class UpdateChoiceDto
{
    public int Id { get; set; }
    public int StoryNodeId { get; set; }
    public int NextStoryNodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int? HealthEffect { get; set; }
}
