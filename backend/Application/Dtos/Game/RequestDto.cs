namespace backend.Application.Dtos.Game;
/// <summary>
/// Dto used for starting a new game.
/// </summary>
public class StartGameRequest
{
    public int UserId { get; set; }
    public string? SaveName { get; set; }
}

/// <summary>
/// Dto used for making a choice in the game. 
/// </summary>
public class MakeChoiceRequest
{
    public int SaveId { get; set; }
    public int ChoiceId { get; set; }
}