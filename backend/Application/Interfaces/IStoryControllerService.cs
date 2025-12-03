using backend.Application.Dtos.Game;
using backend.Application.Dtos.Story;

namespace backend.Application.Interfaces;

/// <summary>
/// Interface for coordinating story flow, choices, dialogues, and player state management for an active game session. 
/// </summary>

public interface IStoryControllerService
{
    #region Story Navigation Methods
    // Navigate between story nodes
    Task<StoryNodeDto> GetCurrentNode(int saveId);
    Task<StoryNodeDto?> NavigateToNode(int saveId, int targetNodeId);
    Task<StoryNodeDto?> GoBack(int saveId);
    Task<StoryNodeDto?> GoForward(int saveId);
    #endregion

    #region Choice Handling Methods
    // Handle player choices and their effects
    Task<StoryNodeDto?> MakeChoice(int saveId, int choiceId);
    Task<IEnumerable<ChoiceDto>> GetAvailableChoices(int saveId);
    #endregion

    #region Dialogue Management Methods
    // Step through dialogues within the current story node
    Task<DialogueDto?> GetNextDialogue(int saveId);
    Task<DialogueDto?> SkipToLastDialogue(int saveId);
    Task<bool> IsDialogueComplete(int saveId);
    #endregion

    #region Health Management Methods
    // Manage player character health
    Task<int> ModifyHealthFromChoice(int playerCharacterId, int healthDelta);
    Task<PlayerCharacterDto> GetPlayerState(int playerCharacterId);
    #endregion

    #region History Tracking Methods
    // Track visited nodes for tracking progress
    Task<List<int>> GetVisitedNodes(int saveId);
    Task<bool> HasVisitedNode(int saveId, int nodeId);
    #endregion

    #region Game Save Methods
    // Retrieve game save details
    Task<GameSaveDto> GetGameSaveById(int saveId);
    #endregion
}
