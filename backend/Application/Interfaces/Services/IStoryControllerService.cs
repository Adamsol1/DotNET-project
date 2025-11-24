using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Application.Dtos;

namespace backend.Application.Interfaces.Services;

public interface IStoryControllerService
{
    #region Story Navigation Methods
    Task<StoryNodeDto> GetCurrentNode(int saveId);
    Task<StoryNodeDto?> NavigateToNode(int saveId, int targetNodeId);
    Task<StoryNodeDto?> GoBack(int saveId);
    Task<StoryNodeDto?> GoForward(int saveId);
    #endregion

    #region Choice Handling Methods
    Task<StoryNodeDto> MakeChoice(int saveId, int choiceId);
    Task<IEnumerable<ChoiceDto>> GetAvailableChoices(int saveId);
    #endregion

    #region Dialogue Management Methods
    Task<DialogueDto?> GetNextDialogue(int saveId);
    Task<DialogueDto?> SkipToLastDialogue(int saveId);
    Task<bool> IsDialogueComplete(int saveId);
    #endregion

    #region Health Management Methods
    Task<int> ModifyHealthFromChoice(int playerCharacterId, int healthDelta);
    Task<PlayerCharacterDto> GetPlayerState(int playerCharacterId);
    #endregion

    #region History Tracking Methods
    Task<List<int>> GetVisitedNodes(int saveId);
    Task<bool> HasVisitedNode(int saveId, int nodeId);
    #endregion
}
