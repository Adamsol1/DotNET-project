namespace backend.Domain.Models;

public class GameSave
{
    //Primary Key
    public int Id { get; set; }
    
    //User connection
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    //PlayerCharacter connection
    public int PlayerCharacterId { get; set; }
    public PlayerCharacter? PlayerCharacter { get; set; }
    
    //Which story-node the player is at
    public int CurrentStoryNodeId { get; set; }
    public StoryNode? CurrentStoryNode { get; set; }

    //Player health
    public int Health { get; set; } = 100;
    
    //History tracking for navigation
    public string VisitedNodeIds { get; set; } = "[]"; // JSON array of visited node IDs
    public int? LastChoiceId { get; set; } // The last choice the player made
    public int CurrentDialogueIndex { get; set; } = 0; // Track which dialogue is currently showing
    
    //Save info
    public String SaveName { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
}