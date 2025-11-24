namespace backend.Application.Dtos;

/*
Instead of creating a new file for each dto, we just group them in 
a file., where I think it makes sense add them in.

// makes it easier to expect where they might be and also reduces the
amount of dto files, as each dto often isnt longer than 10 lines. 
-Ah
*/

// Game session Management, 
// the idea is that if the user creates a new game or fetches a game from the db
// we create or return an object that already have the attributes needed for that task.

public class GameSessionDto 
{
    // player character they create or have created.
    public int PlayerCharacterId { get; set; }
    //the userid the object belongs to.
    public int UserId { get; set; }
    // the story node that they are on, or finished on.
    public int StoryNodeId { get; set; }
    // the date created
    public DateTime DateCreated { get; set; }
    // the update time.
    public DateTime DateUpdated { get; set; }
}

// small game state object
public class MiniGameStateDto
{
    public int PlayerCharacterId { get; set; }
    public int CurrentStoryNodeId { get; set; }
    public int Health { get; set; }
}

public class PlayerCharacterDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public int UserId { get; set; }
    public int CurrentStoryNodeId { get; set; }
}

public class GameStateDto
{
    public int SaveId { get; set; }
    public PlayerCharacterDto PlayerCharacter { get; set; }
    public StoryNodeDto CurrentStoryNode { get; set; }
    public IEnumerable<ChoiceDto> AvailableChoices { get; set; }
    public bool IsGameOver { get; set; }
}

// start game request dto.
public class StartGameRequestDto
{
    public int UserId { get; set; }
    public string SaveName { get; set; }
}

public class ModifyHealthRequestDto
{
    public int choiceId { get; set; }
    public int healthValue { get; set; }
}

// make choice request dto.
public class MakeChoiceRequestDto
{
    public int SaveId { get; set; }
    public int ChoiceId { get; set; }
}

public class GameSaveDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlayerCharacterId { get; set; }
    public string SaveName { get; set; }
    public int CurrentStoryNodeId { get; set; }
    public DateTime LastUpdate { get; set; }
    public int Health { get; set; } = 100;
}

// game state object to track and return the current game state.
public class FullGameStateDto
{
    // player character they are playing as.
    public int PlayerCharacterId { get; set; }
    // the current story node they are on.
    public StoryNodeDto CurrentStoryNode { get; set; }
    // the available choices they can make.
    public List<ChoiceDto> AvailableChoices { get; set; }
    // the dialogues in the current story node.
    public List<DialogueDto> Dialogues { get; set; }
    // the characters in the current story node.
    public List<CharacterDto> CharactersInScene { get; set; }
    // the progress of the game.
    public GameProgressDto Progress { get; set; }
}

// progression object to track and return the progression of the game.
public class GameProgressDto
{
    // the player character they are playing as.
    public int PlayerCharacterId { get; set; }
    // the current story node they are on.
    public int CurrentStoryNodeId { get; set; }
    // the story nodes they have visited.
    public List<int> VisitedStoryNodes { get; set; }
    // the choices they have completed.
    public List<int> CompletedChoices { get; set; }
    // the total choices they have made.
    public int TotalChoicesMade { get; set; }
}

public class UpdateGameSaveRequest
{
    public int? CurrentStoryNodeId { get; set; }
	public List<int>? VisitedNodeIds { get; set; }
}

// make choice dto.
public class MakeChoiceDto
{
    public int PlayerCharacterId { get; set; }
    public int ChoiceId { get; set; }
}

// save for the future.
public class SaveGameDto
{
    public int PlayerCharacterId { get; set; }
    public int CurrentStoryNodeId { get; set; }
}

// move to next story node dto.
public class MoveToNextNodeDto {
    public int PlayerCharacterId { get; set; }
    public int NextStoryNodeId { get; set; }
}

public sealed class MoveToPreviousNodeDto
{
    public int PlayerCharacterId { get; set; }
    public int PreviousStoryNodeId { get; set; }
}
