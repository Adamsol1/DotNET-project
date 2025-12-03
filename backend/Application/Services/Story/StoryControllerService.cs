using System.Text.Json;
using backend.Application.Dtos.Game;
using backend.Application.Dtos.Story;
using backend.Application.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;

namespace backend.Application.Services.Story;

/*
This file combines handling of business logic from mutliple services.
so that the controller just can delegate the task to it. 

this includes calling actions that spans from different services
repositories, and aggregates the data for the controller.

*/

public class StoryControllerService : IStoryControllerService
{
    private readonly IUnitOfWork _uow;
    private readonly IGenService _genService;
    private readonly IStoryService _storyService;
    private readonly IEntityFileLogger _entityLogger;

    public StoryControllerService(
        IUnitOfWork uow, 
        IGenService genService, 
        IStoryService storyService,
        IEntityFileLogger entityLogger)
    {
        _uow = uow;
        _genService = genService;
        _storyService = storyService;
        _entityLogger = entityLogger;
    }

    #region Story Navigation Methods

    /// <summary>
    /// Method to get the current game node that the user is on.
    /// from the storyService.
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
    public async Task<StoryNodeDto> GetCurrentNode(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            // we take in the saveId to know where the user is in the story.
            // and use it to get the current story node id from the game save.    
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            return await _storyService.GetStoryNodeById(gameSave.CurrentStoryNodeId);
        });
    }

    /// <summary>
    /// method to navigate to a specific node in a game by calling 
    /// the node that is is wanted, can be used to jump over nodes 
    /// that the player has visited
    /// </summary>
    /// <param name="saveId"></param>
    /// <param name="targetNodeId"></param>
    /// <returns></returns>
    public async Task<StoryNodeDto?> NavigateToNode(int saveId, int targetNodeId)
    {
        // Excecute wraps the async action into a transaction. that manages, the Unit of work.
        // and handles error, returning the outcom, handling error and returns the result.
        // was an experimental style to write less code and more effective.
        // and make the Excecute reusable, 
        
        // navigates the a specific node through the NavigateToNodeCore
        return await _genService.Execute(async () =>
        {
            return await NavigateToNodeCore(saveId, targetNodeId);
        });
    }

    /// <summary>
    /// handles the actual navigation done by NavigateToNode
    /// validates if the node exists checks the nodes player has visited
    /// and calculates the next node by checking the previous
    /// this method was made early in development, when the logic wasnt advanced.
    /// </summary>
    /// <param name="saveId"></param>
    /// <param name="targetNodeId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// method to find the previous Node, user has visited to go back
    /// we have to keep track of the nodes the player has visited.
    /// and move back two nodes because current node is 1 and previous node is 2.
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// method to move forwards to the next node gets the current node the player is on
    /// and finds the nextNode based on the choice´s nextNodeId
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Method player uses to make and apply their choice. 
    /// validates if the choice is valid and belongs to the story node
    /// adds health effects such as subtractions if the choice has it.
    /// and navigates to the next story node
    /// the method is wrapped in Excecute which ensures transaction and error 
    /// handling
    /// </summary>
    /// <param name="saveId"></param>
    /// <param name="choiceId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

            // Apply health effect from the choice if present
            if (choice.HealthEffect.HasValue && choice.HealthEffect.Value != 0)
            {
                var playerCharacter = await _genService.ValidateEntityExists<PlayerCharacter>(gameSave.PlayerCharacterId);
                playerCharacter.Health = Math.Clamp(playerCharacter.Health + choice.HealthEffect.Value, 0, 100);

                await _uow.PlayerCharacterRepository.Update(playerCharacter);
            }

            // Oppdater historikk på save før hopp
            gameSave.LastChoiceId = choiceId;
            await _uow.GameRepository.Update(gameSave);

            // VIKTIG: kall core (ingen ny transaksjon her)
            var next = await NavigateToNodeCore(saveId, choice.NextStoryNodeId);
            return next;
        });
    }

    /// <summary>
    /// gets all choices that are available, was meant as an administrative method
    /// gets all choices that exists in the current story node the player is on
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Method to fetch and get the next dialogue in a story node.
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Made as an DEBUGG method, to skip faster through dialogues, 
    /// function gets all dialogues and jumps to last in the index.
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// method to make sure and check if a dialogue is complete before
    /// taking action to go to the next node.
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Method to modifi the players health
    /// sets a decided health value and starts the player health as
    /// 0 to make the chosen value the new health
    /// </summary>
    /// <param name="playerCharacterId"></param>
    /// <param name="healthDelta"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets the current state of a player´s character based on the playerCharacter Id linked to the user
    /// such as Id, name and health, which can be used to display in the UI
    /// </summary>
    /// <param name="playerCharacterId"></param>
    /// <returns></returns>
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
            };
        });
    }

    #endregion

    #region History Tracking Methods

    /// <summary>
    /// gets a list of all storynodes that a player has visited
    /// used to track progress and prevent infinate loops as well
    /// as can be used to jump over nodes that is visted.
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// method to check if a user has visited a specific node.
    /// </summary>
    /// <param name="saveId"></param>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public async Task<bool> HasVisitedNode(int saveId, int nodeId)
    {
        var visitedNodes = await GetVisitedNodes(saveId);
        return visitedNodes.Contains(nodeId);
    }

    #endregion

    #region Game Save Methods

    /// <summary>
    /// Method to get complete save information for a game
    /// including the users Id, playerCharacter, currentnode
    /// used to load saves, and displaying information, aswell as checking 
    /// the progress returns a compleete GameSaveDTO
    /// </summary>
    /// <param name="saveId"></param>
    /// <returns></returns>
    public async Task<GameSaveDto> GetGameSaveById(int saveId)
    {
        return await _genService.Execute(async () =>
        {
            var gameSave = await _genService.ValidateEntityExists<GameSave>(saveId);
            
            return new GameSaveDto
            {
                Id = gameSave.Id,
                UserId = gameSave.UserId,
                PlayerCharacterId = gameSave.PlayerCharacterId,
                SaveName = gameSave.SaveName,
                CurrentStoryNodeId = gameSave.CurrentStoryNodeId,
                LastUpdate = gameSave.LastUpdate
            };
        });
    }

    #endregion
}
