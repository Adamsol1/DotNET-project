namespace backend.Domain.Models;
/// <summary>
/// Represents a player character in the story, inheriting from Character.
/// </summary>
public class PlayerCharacter : Character
{
    //Health of the player character
    public int Health { get; set; } = 100;

}