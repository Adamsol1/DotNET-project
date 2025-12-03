using backend.Application.Dtos.Game;
using backend.Application.Dtos.Story;
using backend.Application.Interfaces;
using backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Story;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoryController : ControllerBase
{
    // dependencies in the constructor.
    private readonly IStoryControllerService _storyControllerService;
    private readonly IEntityFileLogger _entityLogger;

    public StoryController(IStoryControllerService storyControllerService, IEntityFileLogger entityFileLogger)
    {
        _storyControllerService = storyControllerService;
        _entityLogger = entityFileLogger;
    }

    // navigational endpoints.

    // this method is meant to get the current story node for a given save id.
    // used to display the current story node the user is on.
    [HttpGet("current/{saveId}")]
    public async Task<ActionResult<StoryNodeDto>> GetCurrentNode(int saveId)
    {
        try {

            // get the current node for the given save id.
            var currentNode = await _storyControllerService.GetCurrentNode(saveId);
            
            // if the current node is not found, return a not found status.
            if (currentNode == null) {
                await _entityLogger.LogAsync(
                    "get current node error not found",
                    new { SaveId = saveId, Timestamp = DateTime.UtcNow },
                    LogCategories.Story
                );
                return NotFound("Current node not found");
            }

            // Log successful retrieval
            await _entityLogger.LogAsync(
                "get current node successful",
                currentNode,
                LogCategories.Story
            );

            // return the current node.
            return Ok(currentNode);
        }
        catch (Exception ex) {
            await _entityLogger.LogAsync(
                "get current node error",
                new { SaveId = saveId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return StatusCode(500, "Error getting current node");
        }
    }
    
    // move to a specific storyNode
    [HttpPost("nav/{nodeId}")]
    public async Task<ActionResult<StoryNodeDto>> NavigateToNode(int saveId, int nodeId)
    {
        try {
            // get the node based on the saveid / session and the node id.
            var node = await _storyControllerService.NavigateToNode(saveId, nodeId);
            
            // if the node is not found, return a not found status.
            if (node == null) {
                await _entityLogger.LogAsync(
                    "navigate to node error not found",
                    new { SaveId = saveId, NodeId = nodeId, Timestamp = DateTime.UtcNow },
                    LogCategories.Story
                );
                return NotFound("Node not found");
            }

            // Log successful navigation
            await _entityLogger.LogAsync(
                "navigate to node successful",
                node,
                LogCategories.Story
            );

            // return the node.
            return Ok(node);
            
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "navigate to node error",
                new { SaveId = saveId, NodeId = nodeId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to navigate to node");
        }
    }

    // move to a previous storyNode
    [HttpPost("back/{saveId}")]
    public async Task<ActionResult<StoryNodeDto>> GoBack(int saveId)
    {
        try {
            // get the previous node for the given save id.
            var previousNode = await _storyControllerService.GoBack(saveId);

            if (previousNode == null) {
                await _entityLogger.LogAsync(
                    "go back error not found",
                    new { SaveId = saveId, Timestamp = DateTime.UtcNow },
                    LogCategories.Story
                );
                return NotFound("Previous node not found");
            }

            // Log successful navigation
            await _entityLogger.LogAsync(
                "go back successful",
                previousNode,
                LogCategories.Story
            );

            // return the previous node.
            return Ok(previousNode);
        } catch (Exception ex) {
            await _entityLogger.LogAsync(
                "go back error",
                new { SaveId = saveId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to go back");
        }
    }

    // move to a next storyNode
    [HttpPost("next/{saveId}")]
    public async Task<ActionResult<StoryNodeDto>> GoForward(int saveId)
    {
        try {
            // get the next node for the given save id.
            var nextNode = await _storyControllerService.GoForward(saveId);

            if (nextNode == null) {
                await _entityLogger.LogAsync(
                    "go forward error not found",
                    new { SaveId = saveId, Timestamp = DateTime.UtcNow },
                    LogCategories.Story
                );
                return NotFound("Next node not found");
            }

            // Log successful navigation
            await _entityLogger.LogAsync(
                "go forward successful",
                nextNode,
                LogCategories.Story
            );

            // return the next node.
            return Ok(nextNode);
        } 
        catch (Exception ex) 
        {
            await _entityLogger.LogAsync(
                "go forward error",
                new { SaveId = saveId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to go forward");
        }
    }

    // choice handling endpoints.

    // make a choice should be here instead of in the game controller.
    [HttpPost("choice")]
    public async Task<ActionResult<MakeChoiceResponseDto>> MakeChoice([FromBody] MakeChoiceRequestDto request)
    {
        try {
            // make the choice.
            var storyNode = await _storyControllerService.MakeChoice(request.SaveId, request.ChoiceId);
            
            // Get the game save to retrieve player character ID
            var gameSave = await _storyControllerService.GetGameSaveById(request.SaveId);
            
            // Get the updated player state
            var playerState = await _storyControllerService.GetPlayerState(gameSave.PlayerCharacterId);
            
            // Get available choices for the new node
            var availableChoices = await _storyControllerService.GetAvailableChoices(request.SaveId);
            
            // Log successful choice
            await _entityLogger.LogAsync(
                "make choice successful",
                new { SaveId = request.SaveId, ChoiceId = request.ChoiceId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            
            // Return complete response with node, choices, and updated player state
            return Ok(new MakeChoiceResponseDto
            {
                CurrentStoryNode = storyNode,
                AvailableChoices = availableChoices,
                PlayerCharacter = playerState
            });
        } 
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "make choice error not found",
                new { SaveId = request.SaveId, ChoiceId = request.ChoiceId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Game save {request.SaveId} not found");
        }
        catch (InvalidOperationException ex)
        {
            await _entityLogger.LogAsync(
                "make choice error invalid",
                new { SaveId = request.SaveId, ChoiceId = request.ChoiceId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Choice does not belong to current node");
        }
        catch (Exception ex) 
        {
            await _entityLogger.LogAsync(
                "make choice error",
                new { SaveId = request.SaveId, ChoiceId = request.ChoiceId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to make choice");
        }
   
    }

    // get available choices for the current story node, the player is on.
    [HttpGet("choices/{saveId}")]
    public async Task<ActionResult<IEnumerable<ChoiceDto>>> GetAvailableChoices(int saveId)
    {
        try {

            // get the available choices for the current story node, the player is on.
            var choices = await _storyControllerService.GetAvailableChoices(saveId);

            // Log successful retrieval
            await _entityLogger.LogAsync(
                "get available choices successful",
                new { SaveId = saveId, ChoiceCount = choices.Count(), Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );

            // return the choices.
            return Ok(choices);
        }
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "get available choices error not found",
                new { SaveId = saveId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Game save {saveId} not found");
        }
    }

    // get the next dialogue for the current story node, the player is on.
    [HttpGet("dialogue/next/{saveId}")]
    public async Task<ActionResult<DialogueDto>> GetNextDialogue(int saveId)
    {
        try {
            // get the next dialogue for the current story node, the player is on.
            var dialogue = await _storyControllerService.GetNextDialogue(saveId);

            // Log successful retrieval
            await _entityLogger.LogAsync(
                "get next dialogue successful",
                dialogue,
                LogCategories.Story
            );

            // return the dialogue.
            return Ok(dialogue);
        }
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "get next dialogue error not found",
                new { SaveId = saveId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Game save {saveId} not found");
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "get next dialogue error",
                new { SaveId = saveId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to get next dialogue");
        }
    }

    // NOTE: skip/check-dialogue endpoints were removed as they were unused.

    // modify health from a choice.
    [HttpPost("health")]
    public async Task<ActionResult<int>> ModifyHealth([FromBody] ModifyHealthRequestDto request)
    {
        try
        {
            var newHealth = await _storyControllerService.ModifyHealthFromChoice(request.choiceId, request.healthValue);
            
            // Log successful modification
            await _entityLogger.LogAsync(
                "modify health successful",
                new { ChoiceId = request.choiceId, NewHealth = newHealth, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            
            return Ok(newHealth);
        }
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "modify health error not found",
                new { ChoiceId = request.choiceId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Choice {request.choiceId} not found");
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "modify health error",
                new { ChoiceId = request.choiceId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to modify health");
        }
    }

    // get the current state of a player character.
    [HttpGet("player/{playerCharacterId}")]
    public async Task<ActionResult<PlayerCharacterDto>> GetPlayerState(int playerCharacterId)
    {
        try
        {
            var playerState = await _storyControllerService.GetPlayerState(playerCharacterId);
            
            // Log successful retrieval
            await _entityLogger.LogAsync(
                "get player state successful",
                playerState,
                LogCategories.Story
            );
            
            return Ok(playerState);
        }
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "get player state error not found",
                new { PlayerCharacterId = playerCharacterId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Player character {playerCharacterId} not found");
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "get player state error",
                new { PlayerCharacterId = playerCharacterId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return StatusCode(500, "Failed to get player state");
        }
    }

    // get the nodes the player has visited.
    [HttpGet("history/{saveId}")]
    public async Task<ActionResult<List<int>>> GetVisitedNodes(int saveId)
    {
        try
        {
            var visitedNodes = await _storyControllerService.GetVisitedNodes(saveId);
            
            // Log successful retrieval
            await _entityLogger.LogAsync(
                "get visited nodes successful",
                new { SaveId = saveId, NodeCount = visitedNodes.Count, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            
            return Ok(visitedNodes);
        }
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "get visited nodes error not found",
                new { SaveId = saveId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Game save {saveId} not found");
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "get visited nodes error",
                new { SaveId = saveId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to get visited nodes");
        }
    }

    // check if the player has visited a specific node.
    [HttpGet("history/{saveId}/{nodeId}")]
    public async Task<ActionResult<bool>> HasVisitedNode(int saveId, int nodeId)
    {
        try
        {
            var hasVisited = await _storyControllerService.HasVisitedNode(saveId, nodeId);
            
            // Log successful check
            await _entityLogger.LogAsync(
                "check visited node successful",
                new { SaveId = saveId, NodeId = nodeId, HasVisited = hasVisited, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            
            return Ok(hasVisited);
        }
        catch (KeyNotFoundException)
        {
            await _entityLogger.LogAsync(
                "check visited node error not found",
                new { SaveId = saveId, NodeId = nodeId, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return NotFound($"Game save {saveId} not found");
        }
        catch (Exception ex)
        {
            await _entityLogger.LogAsync(
                "check visited node error",
                new { SaveId = saveId, NodeId = nodeId, Error = ex.Message, Timestamp = DateTime.UtcNow },
                LogCategories.Story
            );
            return BadRequest("Failed to check visited status");
        }
    }
}
