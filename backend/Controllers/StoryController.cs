using Microsoft.AspNetCore.Mvc;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoryController : ControllerBase
{
    // dependencies in the constructor.
    private readonly IStoryControllerService _storyControllerService;
    private readonly ILogger<StoryController> _logger;

    public StoryController(IStoryControllerService storyControllerService, ILogger<StoryController> logger)
    {
        _storyControllerService = storyControllerService;
        _logger = logger;
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
            if (currentNode == null) return NotFound("Current node not found");

            // return the current node.
            return Ok(currentNode);
        }
        catch (Exception ex) {
            _logger.LogError(ex, " Error getting current node for save id {saveId}", saveId);
            return StatusCode(500, "Error getting current node for save id {saveId}: " + ex.Message);
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
            if (node == null) return NotFound("Node not found");

            // return the node.
            return Ok(node);
            
        } catch (Exception ex) {
            _logger.LogError(ex, " Error navigating to node {nodeId} for save id {saveId}", nodeId, saveId);
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

            if (previousNode == null) return NotFound("Previous node not found");

            // return the previous node.
            return Ok(previousNode);
        } catch (Exception ex) {
            _logger.LogError(ex, " Error going back for save id {saveId}", saveId);
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

            if (nextNode == null) return NotFound("Next node not found");

            // return the next node.
            return Ok(nextNode);
        } 
        catch (Exception ex) 
        {
            _logger.LogError(ex, " Error going forward for save id {saveId}", saveId);
            return BadRequest("Failed to go forward: " + ex.Message);
        }
    }

    // choice handling endpoints.

    // make a choice should be here instead of in the game controller.
    [HttpPost("choice")]
    public async Task<ActionResult<StoryNodeDto>> MakeChoice([FromBody] MakeChoiceRequestDto request)
    {
        try {
            // make the choice.
            Console.WriteLine("StroyController saveId check: " + request.SaveId);
            var choice = await _storyControllerService.MakeChoice(request.SaveId, request.ChoiceId);
            Console.WriteLine("Stroycontroller - check choice obj. choice is: " + choice);
            // error handling for validation is done in the service.
            // return the choice.
            return Ok(choice);
        } 
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Game save {SaveId} not found", request.SaveId);
            return NotFound($"Game save {request.SaveId} not found");
        }
        catch (InvalidOperationException ex)
        {
            
            Console.WriteLine("Stroycontroller - " + ex.Message);
            _logger.LogWarning("Choice {ChoiceId} does not belong to current node for save {SaveId}", request.ChoiceId, request.SaveId);
            return BadRequest("Choice does not belong to current node ");
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, " Error making choice for save id {saveId} and choice id {choiceId}", request.SaveId, request.ChoiceId);
            return BadRequest("Failed to make choice: " + ex.Message);
        }
   
    }

    // get available choices for the current story node, the player is on.
    [HttpGet("choices/{saveId}")]
    public async Task<ActionResult<IEnumerable<ChoiceDto>>> GetAvailableChoices(int saveId)
    {
        try {

            // get the available choices for the current story node, the player is on.
            var choices = await _storyControllerService.GetAvailableChoices(saveId);

            // return the choices.
            return Ok(choices);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Game save {SaveId} not found", saveId);
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


            // return the dialogue.
            return Ok(dialogue);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Game save {SaveId} not found", saveId);
            return NotFound($"Game save {saveId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, " Error getting next dialogue for save id {saveId}", saveId);
            return BadRequest("Failed to get next dialogue: " + ex.Message);
        }
    }

    // modify health from a choice.
    [HttpPost("health")]
    public async Task<ActionResult<int>> ModifyHealth([FromBody] ModifyHealthRequestDto request)
    {
        try
        {
            var newHealth = await _storyControllerService.ModifyHealthFromChoice(request.choiceId, request.healthValue);
            return Ok(newHealth);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Choice {ChoiceId} not found", request.choiceId);
            return NotFound($"Choice {request.choiceId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying health for choice {ChoiceId}", request.choiceId);
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
            return Ok(playerState);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Player character {PlayerCharacterId} not found", playerCharacterId);
            return NotFound($"Player character {playerCharacterId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player state for player {PlayerCharacterId}", playerCharacterId);
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
            return Ok(visitedNodes);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Game save {SaveId} not found", saveId);
            return NotFound($"Game save {saveId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visited nodes for save {SaveId}", saveId);
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
            return Ok(hasVisited);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Game save {SaveId} not found", saveId);
            return NotFound($"Game save {saveId} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if node {NodeId} visited for save {SaveId}", nodeId, saveId);
            return BadRequest("Failed to check visited status");
        }
    }
}
