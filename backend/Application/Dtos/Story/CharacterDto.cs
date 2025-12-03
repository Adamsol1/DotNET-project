namespace backend.Application.Dtos.Story;

// NPC character dto.
/// <summary>
/// DTO for a character or npc in the story. Includes the characters:
/// - Id
/// - Name
/// - Description
/// - ImageUrl
/// - Dialogues associated with the character
/// </summary>
public sealed class CharacterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<DialogueDto> Dialogues { get; set; } = new List<DialogueDto>();
}

/// <summary>
/// DTO used for creating a new character. 
/// </summary>
public sealed class CreateCharacterDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO used for updating the current character. 
/// </summary>
public sealed class UpdateCharacterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}