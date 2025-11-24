using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using backend.Domain.Models;
using System.Text.Json;

namespace backend.Application;

public class StoryControllerService : IStoryControllerService
{
    private readonly IUnitOfWork _uow;
    private readonly IGenService _genService;
    private readonly IStoryService _storyService;
    private readonly ILogger<StoryControllerService> _logger;

    public StoryControllerService(
        IUnitOfWork uow, 
        IGenService genService, 
        IStoryService storyService,
        ILogger<StoryControllerService> logger)
    {
        _uow = uow;
        _genService = genService;
        _storyService = storyService;
        _logger = logger;
    }

    #region Story Navigation Methods

    public async Task<StoryNodeDto> GetCurrentNode(int saveId)
    {
        _logger.LogInformation("Getting current node for saveId: {SaveId}", saveId);
        return await _genService.Execute(async () =>
        {
            // we take in the saveId to know where the user is in the story.
            // and use it to get the current story node id from the game save.    
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            _logger.LogInformation("Found gameSave with CurrentStoryNodeId: {NodeId}", gameSave.CurrentStoryNodeId);
            return await _storyService.GetStoryNodeById(gameSave.CurrentStoryNodeId);
        });
    }
    public async Task<StoryNodeDto?> NavigateToNode(int saveId, int targetNodeId)
    {
        return await _genService.Execute(async () =>
        {
            return await NavigateToNodeCore(saveId, targetNodeId);
        });
    }

    private async Task<StoryNodeDto?> NavigateToNodeCore(int saveId, int targetNodeId)
    {
            // check if the entity exists.
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            
            // check if the target node exists.
            if (!await _genService.CheckStoryNodeExists(targetNodeId))
                return null;

            // get the visited nodes by using the saveId
            // it acts as an session id, which has the current node id.
            // from this we then keep track of the nodes the player has visited.
            var visitedNodes = JsonSerializer.Deserialize<List<int>>(gameSave.VisitedNodeIds) 
                ?? new List<int>();
            
            // if the visited nodes list is empty or the last node is not the current node, 
            // add the current node to the list.
            if (visitedNodes.Count == 0 || visitedNodes.Last() != gameSave.CurrentStoryNodeId)
            {
                visitedNodes.Add(gameSave.CurrentStoryNodeId);
            }

            // update the game save with the new current node id.
            // and the visited nodes list.
            // and the last update time.
            // also reset the dialogue index since we're in a new node.
            gameSave.CurrentStoryNodeId = targetNodeId;
            gameSave.VisitedNodeIds = JsonSerializer.Serialize(visitedNodes);
            gameSave.CurrentDialogueIndex = 0; // Reset dialogue index for new node
            gameSave.LastUpdate = DateTime.UtcNow;
            
            await _uow.GameRepository.Update(gameSave);
            return await _storyService.GetStoryNodeById(targetNodeId);
        
    }

    // to find the previous Node we have to keep track of the nodes the player has visited.
    // and move back two nodes because current node is 1 and previous node is 2.

    public async Task<StoryNodeDto?> GoBack(int saveId)
    {
        // we use the excecute method from the GenService to handle the transaction.
        // execute is wrapper method that handles the transaction code.
        // and delegates the work to its called, which is goBack function.
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);

            // check if there is a last choice to go back from
            if (!gameSave.LastChoiceId.HasValue)
                return null; // Can't go back from the first node
            
            // get the last choice that was made
            var lastChoice = await _genService.ValidateEntityExists<Choice>(gameSave.LastChoiceId.Value);
            
            // the previous node is the node that the choice belongs to
            var previousNodeId = lastChoice.StoryNodeId;
            
            // update the visited nodes list by removing the last entry
            var visitedNodes = JsonSerializer.Deserialize<List<int>>(gameSave.VisitedNodeIds) 
                ?? new List<int>();
            
            // clear the last choice ID since we're going back
            gameSave.LastChoiceId = null;
            
            // navigate to the previous node without adding to visited list
            gameSave.CurrentStoryNodeId = previousNodeId;
            gameSave.LastUpdate = DateTime.UtcNow;
            await _uow.GameRepository.Update(gameSave);
            
            // return the previous node
            return await _storyService.GetStoryNodeById(previousNodeId);
        });
    }

    // go forward works the same as above just in the opposite direction.
    
    public async Task<StoryNodeDto?> GoForward(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            // validate the game save exists.
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);

            // get the current node id.
            var currentNodeId = gameSave.CurrentStoryNodeId;

            // get the choices that belongs to the current node.
            var choices = await _storyService.GetChoicesInStoryNode(currentNodeId);

            // check if the choices are empty.
            if (!choices.Any()) return null;

            // get the first choice and its next node
            var firstChoice = choices.First();
            var nextNodeId = firstChoice.NextStoryNodeId;
            
            // get the actual choice entity to store its ID
            var choiceEntity = await _genService.ValidateEntityExists<Choice>(firstChoice.Id);
            
            // store the choice ID so we can go back
            gameSave.LastChoiceId = choiceEntity.Id;
            
            // navigate to the next node
            return await NavigateToNode(saveId, nextNodeId);
        });
    }

    #endregion

    #region Choice Handling Methods

    
    public async Task<StoryNodeDto?> MakeChoice(int saveId, int choiceId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            var choice   = await _genService.ValidateEntityExists<Choice>(choiceId);

         
            
            // Valider at choice tilhører current node (din eksisterende logikk)
            if (choice.StoryNodeId != gameSave.CurrentStoryNodeId)
                throw new InvalidOperationException(
                    $"Choice {choiceId} does not belong to node {gameSave.CurrentStoryNodeId}");

            // Oppdater historikk på save før hopp
            gameSave.LastChoiceId = choiceId;
            await _uow.GameRepository.Update(gameSave);

            // VIKTIG: kall core (ingen ny transaksjon her)
            var next = await NavigateToNodeCore(saveId, choice.NextStoryNodeId);
            return next;
        });
    }

    /*
    public async Task<StoryNodeDto> MakeChoice(int saveId, int choiceId)
    {
        return await _genService.Execute(async () =>
            {
                Console.WriteLine("Staring MakeChoice in StoryControllerService");
                var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
                var choice = await _genService.ValidateEntityExists<Choice>(choiceId);

                try
                {
                    Console.WriteLine("If-statement in MakeChoice in StoryControllerService");
                    if (choice.StoryNodeId == gameSave.CurrentStoryNodeId)
                    {
                        Console.WriteLine("Ending MakeChoice in StoryControllerService");
                        return await NavigateToNode(saveId, choice.NextStoryNodeId);
                    }
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(
                        $"Choice {choiceId} does not belong to current node {gameSave.CurrentStoryNodeId}: " + e);

                    throw;
                }

                return null;
            }
       );

    }
    */

    public async Task<IEnumerable<ChoiceDto>> GetAvailableChoices(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            return await _storyService.GetChoicesInStoryNode(gameSave.CurrentStoryNodeId);
        });
    }



    #endregion

    #region Dialogue Management Methods

    public async Task<DialogueDto?> GetNextDialogue(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            var dialogues = await _storyService.GetDialoguesInStoryNode(gameSave.CurrentStoryNodeId);
            
            // get dialogues ordered by their order property
            var orderedDialogues = dialogues.OrderBy(d => d.Order).ToList();
            
            // check if there are more dialogues to show
            if (gameSave.CurrentDialogueIndex >= orderedDialogues.Count)
                return null; // No more dialogues
            
            // get the dialogue at the current index
            var dialogue = orderedDialogues[gameSave.CurrentDialogueIndex];
            
            // increment the dialogue index for next time
            gameSave.CurrentDialogueIndex++;
            await _uow.GameRepository.Update(gameSave);
            
            return dialogue;
        });
    }

    public async Task<DialogueDto?> SkipToLastDialogue(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            var dialogues = await _storyService.GetDialoguesInStoryNode(gameSave.CurrentStoryNodeId);
            
            // get dialogues ordered by their order property
            var orderedDialogues = dialogues.OrderBy(d => d.Order).ToList();
            
            if (!orderedDialogues.Any())
                return null;
            
            // set the dialogue index to the last dialogue
            gameSave.CurrentDialogueIndex = orderedDialogues.Count - 1;
            await _uow.GameRepository.Update(gameSave);
            
            return orderedDialogues.Last();
        });
    }

    public async Task<bool> IsDialogueComplete(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            var dialogues = await _storyService.GetDialoguesInStoryNode(gameSave.CurrentStoryNodeId);
            
            // get dialogues ordered by their order property
            var orderedDialogues = dialogues.OrderBy(d => d.Order).ToList();
            
            // check if we've seen all dialogues
            return gameSave.CurrentDialogueIndex >= orderedDialogues.Count;
        });
    }

    #endregion

    #region Health Management Methods

    // ah I dont really

    public async Task<int> ModifyHealthFromChoice(int playerCharacterId, int healthDelta)
    {
        return await _genService.Execute(async () =>
        {
            var player = await _genService.ValidateEntityExists<PlayerCharacter>(playerCharacterId);
            player.Health = Math.Max(0, player.Health + healthDelta);
            await _uow.PlayerCharacterRepository.Update(player);
            return player.Health;
        });
    }

    public async Task<PlayerCharacterDto> GetPlayerState(int playerCharacterId)
    {
        return await _genService.Execute(async () =>
        {
            // we validate if the player character exists.
            var player = await _genService.ValidateEntityExists<PlayerCharacter>(playerCharacterId);

            // return the player character dto.
            return new PlayerCharacterDto
            {
                Id = player.Id,
                Name = player.Name,
                Health = player.Health,
                CurrentStoryNodeId = player.CurrentStoryNodeId
            };
        });
    }

    #endregion

    #region History Tracking Methods

    public async Task<List<int>> GetVisitedNodes(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            

            // 
            var visitedNodes = JsonSerializer.Deserialize<List<int>>(gameSave.VisitedNodeIds) 
                ?? new List<int>();
        
            // return the visited nodes.
            return visitedNodes;
        });
    }

    public async Task<bool> HasVisitedNode(int saveId, int nodeId)
    {
        var visitedNodes = await GetVisitedNodes(saveId);
        return visitedNodes.Contains(nodeId);
    }

    #endregion
}
