using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using backend.Domain.Models;
using Serilog;

namespace backend.Application;

public class StoryService : IStoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IGenService _genService;
    private readonly ILogger<StoryService> _logger;

    // constructor
    public StoryService(IUnitOfWork uow, IGenService genService, ILogger<StoryService> logger)
    {
        _logger = logger;
        _uow = uow;
        _genService = genService;
    }

    // get story node by id
    public async Task<StoryNodeDto> GetStoryNodeById(int id) 
    {
        try
        {
            var storyNode = await _genService.ValidateEntityExists<StoryNode>(id);
            
            var dialogues = await _uow.DialogueRepository
                .GetAllByStoryNodeWithCharacter(id);
        
            var choices = await _uow.StoryNodeRepository.GetAllChoicesOfStoryNode(id);

            return _genService.MapStoryNode(storyNode, dialogues, choices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get story node by id: {Id}", id);
            throw;
        }
    }

    // get all story nodes
    public async Task<IEnumerable<StoryNodeDto>> GetAllStoryNodes() {
        // validate the request

        try {
            // begin transaction
            await _uow.BeginAsync();

            // get all story nodes
            var storyNodes = await _uow.StoryNodeRepository.GetAll();

            if (storyNodes == null) {
                _logger.LogWarning("[Storyservice] StoryNode not found");
                throw new Exception("Story nodes not found");
            }

            // fetch dialoges & choices for each story node and add it to a list.
            var result = new List<StoryNodeDto>();
            foreach (var storyNode in storyNodes) {
                var dialogues = await _uow.DialogueRepository
                    .GetAllByStoryNodeWithCharacter(storyNode.Id);
                var choices = await _uow.StoryNodeRepository
                    .GetAllChoicesOfStoryNode(storyNode.Id);
                // error handling later
                result.Add(_genService.MapStoryNode(storyNode, dialogues, choices));
            }

            // since result array contains a return object for each storyNode, 
            // we just return that array
            return result;
        } catch (Exception ex) {
            // rollback, & error handling.
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error retrieving all StoryNodes");
            throw;
        }
    }

    // create a story node
    public async Task<StoryNodeDto> CreateStoryNode(CreateStoryNodeDto request) {
        try {

            // begin transaction
            await _uow.BeginAsync();

            var storyNode = new StoryNode {
                Title = request.Title,
                Description = request.Description,
                BackgroundUrl = request.BackgroundUrl,
            };

            // create in the repository
            await _uow.StoryNodeRepository.Create(storyNode);
            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return the story? or should we return a string? for know i just return the storyNode.
            return _genService.MapStoryNode(storyNode, new List<Dialogue>(), new List<Choice>());

        } catch (Exception ex) {
            // rollback, & error handling.
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error creating StoryNode");
            throw;
        }
    }

    // update a story node
    public async Task<StoryNodeDto> UpdateStoryNode(UpdateStoryNodeDto request) {
        try {
            // begin transaction
            await _uow.BeginAsync();

            var storyNode = await _uow.StoryNodeRepository.GetById(request.Id);

            if (storyNode == null) {
                _logger.LogWarning("[Storyservice] StoryNode with id {StoryNodeId} not found", request.Id);
                throw new Exception("Story node not found");
            }

            storyNode.Title = request.Title;
            storyNode.Description = request.Description;
            storyNode.BackgroundUrl = request.BackgroundUrl;

            // update in the repository
            await _uow.StoryNodeRepository.Update(storyNode);
            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // get the dialogues & choices that belongs to the storyNode
            var dialogues = await _uow.StoryNodeRepository.GetAllDialoguesOfStoryNode(request.Id);
            var choices = await _uow.StoryNodeRepository.GetAllChoicesOfStoryNode(request.Id);

            // return the storyNode
            return _genService.MapStoryNode(storyNode, dialogues, choices);
        } catch (Exception ex) {
            // rollback, & error handling.
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error updating StoryNode with id {StoryNodeId}", request.Id);
            throw;
        }
    }

    public async Task<bool> DeleteStoryNode(int id)
    {
        try {
            await _uow.BeginAsync();

            // search in the db for the story node
            var storyNode = await _uow.StoryNodeRepository.GetById(id);
            if (storyNode == null) {
                _logger.LogWarning("[Storyservice] StoryNode with id {StoryNodeId} not found", id);
                throw new Exception("Story node not found");
            }

            // delete the story node
            await _uow.StoryNodeRepository.Delete(id);
            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return true;
        } catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error deleting StoryNode with id {StoryNodeId}", id);
            throw;
        }
    }

    // create a dialogue
    public async Task<DialogueDto> CreateDialogue(CreateDialogueDto request)
    {
        try {
            await _uow.BeginAsync();

            var dialogue = new Dialogue {
                Text = request.Text,
                CharacterId = request.CharacterId,
                StoryNodeId = request.StoryNodeId,
                Order = request.Order,
            };

            await _uow.DialogueRepository.Create(dialogue);
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // Reload with Character included
            var dialogueWithCharacter = await _uow.DialogueRepository
                .GetByIdWithCharacter(dialogue.Id);
        
            return _genService.MapDialogue(dialogueWithCharacter!);
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error creating Dialogue");
            throw;
        }
    }

    // update a dialogue
    public async Task<DialogueDto> UpdateDialogue(UpdateDialogueDto request) {
        try {
            await _uow.BeginAsync();

            var dialogue = await _uow.DialogueRepository.GetById(request.Id);
            if (dialogue == null) {
                _logger.LogWarning("[Storyservice] Dialogue with id {DialogueId} not found", request.Id);
                throw new Exception("Dialogue not found");
            }

            dialogue.Id = request.Id;
            dialogue.Text = request.Text;
            dialogue.CharacterId = request.CharacterId;
            dialogue.StoryNodeId = request.StoryNodeId;
            dialogue.Order = request.Order;

            await _uow.DialogueRepository.Update(dialogue);
            await _uow.SaveAsync();
            await _uow.CommitAsync();
            
            var dialogueWithCharacter = await _uow.DialogueRepository
                .GetByIdWithCharacter(dialogue.Id);

            return _genService.MapDialogue(dialogueWithCharacter!);
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error updating dialogue with id {DialogueId}", request.Id);
            throw;
        }
    }

    // get the dialogues in a story node
    public async Task<IEnumerable<DialogueDto>> GetDialoguesInStoryNode(int storyNodeId)
    {
        try {
            await _uow.BeginAsync();

            // Use the new method with Character included
            var dialogues = await _uow.DialogueRepository
                .GetAllByStoryNodeWithCharacter(storyNodeId);
        
            return dialogues.Select(_genService.MapDialogue);
        } catch (Exception ex) {
            _logger.LogError(ex, "[Storyservice] Error retrieving dialogues from StoryNode with id {StoryNodeId}", storyNodeId);
            throw;
        }
    }

    // delete a dialogue
    public async Task<bool> DeleteDialogue(int id)
    {
        try {
            await _uow.BeginAsync();
            await _uow.DialogueRepository.Delete(id);
            await _uow.SaveAsync();
            await _uow.CommitAsync();
            return true;
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error deleting dialogue with id {DialogueId}", id);
            throw;
        }
    }

    // create a choice
    public async Task<ChoiceDto> CreateChoice(CreateChoiceDto request)
    {
        try {
            await _uow.BeginAsync();

            // create the choice object
            var choice = new Choice {
                StoryNodeId = request.StoryNodeId,
                NextStoryNodeId = request.NextStoryNodeId,
                Text = request.Text,
            };

            // save to the repository
            await _uow.ChoiceRepository.Create(choice);
            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return the choice
            return _genService.MapChoice(choice);

        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error creating choice");
            throw;
        }
    }

    // update a choice
    public async Task<ChoiceDto> UpdateChoice(UpdateChoiceDto request)
    {
        try {
            await _uow.BeginAsync();

            // get the choice from the repository
            var choice = await _uow.ChoiceRepository.GetById(request.Id);
            if (choice == null)
            {
                _logger.LogWarning("[Storyservice] Choice with id {ChoiceId} not found", request.Id);
                throw new Exception("Choice not found");
            }

            // update the choice
            choice.StoryNodeId = request.StoryNodeId;
            choice.NextStoryNodeId = request.NextStoryNodeId;
            choice.Text = request.Text;

            // update in the repository
            await _uow.ChoiceRepository.Update(choice);
            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return the choice
            return _genService.MapChoice(choice);
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error updating choice with id {ChoiceId}", request.Id);
            throw;
        }
    }
    
    // get choices in a story node
    public async Task<IEnumerable<ChoiceDto>> GetChoicesInStoryNode(int storyNodeId)
    {
        try {
            await _uow.BeginAsync();

            // get the choices from the repository
            var choices = await _uow.StoryNodeRepository.GetAllChoicesOfStoryNode(storyNodeId);
            if (choices == null)
            {
                _logger.LogWarning("[Storyservice] Choices with StoryNodeId {StoryNodeId} not found", storyNodeId);
                throw new Exception("Choices not found");
            }
            // return the choices to DTO object
            return choices.Select(_genService.MapChoice);
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error retrieving choices with StoryNodeId {StoryNodeId}", storyNodeId);
            throw;
        }
    }

    // delete a choice
    public async Task<bool> DeleteChoice(int id)
    {
        try {
            await _uow.BeginAsync();
            await _uow.ChoiceRepository.Delete(id);
            await _uow.SaveAsync();
            await _uow.CommitAsync();
            return true;
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error deleting choice with id {ChoiceId}", id);
            throw;
        }
    }

    // character methods

    // get a character by id
    public async Task<CharacterDto> GetCharacterById(int id)
    {
        try {
            await _uow.BeginAsync();

            // get the character from the repository
            var character = await _uow.CharacterRepository.GetById(id);

            if (character == null)
            {
                _logger.LogWarning("[Storyservice] Character with id {CharacterId} not found", id);
                throw new Exception("Character not found");
            }

            // get the characters dialogues
            var dialogues = await _uow.CharacterRepository.GetAllDialoguesOfCharacter(id);
            if (dialogues == null)
            {
                _logger.LogWarning("[Storyservice] Dialogues with id {DialogueId} not found", id);
                throw new Exception("Dialogues not found");
            }

            // return the character
            return _genService.MapCharacter(character, dialogues);
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error retrieving character with id {CharacterId}", id);
            throw;
        }
    }

    // get all characters
    public async Task<IEnumerable<CharacterDto>> GetAllCharacters()
    {
        try {
            await _uow.BeginAsync();

            var characters = await _uow.CharacterRepository.GetAll();
            if (characters == null)
            {
                _logger.LogWarning("[Storyservice] Characters not found");
                throw new Exception("Characters not found");
            }

            // add the characters to an array
            var result = new List<CharacterDto>();

            // get all the dialogoes for the characters
            foreach (var character in characters) {
                var dialogues = await _uow.CharacterRepository.GetAllDialoguesOfCharacter(character.Id);
                if (dialogues == null)
                {
                    _logger.LogWarning("[Storyservice] Dialogue for character with id {CharacterId} not found", character.Id);
                    throw new Exception("Dialogues not found");
                } 

                result.Add(_genService.MapCharacter(character, dialogues));
            }

            // return the result
            return result;
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error retrieving characters");
            throw;
        }
    }

    // get the characters in a story node
    public async Task<IEnumerable<CharacterDto>> GetCharactersInStoryNode(int storyNodeId)
    {
        try {
            await _uow.BeginAsync();

            // get the characters from the repository
            var characters = await _uow.StoryNodeRepository.GetAllCharactersOfStoryNode(storyNodeId);
            if (characters == null)
            {
                _logger.LogWarning("[Storyservice] Characters from StoryNode with id {StoryNodeId} not found", storyNodeId);
                throw new Exception("Characters not found");
            }

            // add the characters to an array
            var result = new List<CharacterDto>();
            // get all the dialogoes for the characters
            foreach (var character in characters) {
                var dialogues = await _uow.CharacterRepository.GetAllDialoguesOfCharacter(character.Id);
                if (dialogues == null)
                {
                    _logger.LogWarning("[Storyservice] Dialogues from character with id {CharacterId} not found", character.Id);
                    throw new Exception("Dialogues not found");
                }
                
                result.Add(_genService.MapCharacter(character, dialogues));
            }

            // return the result
            return result;
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error retrieving characters from StoryNode with id {StoryNodeId}", storyNodeId);
            throw;
        }
    }

    // create a character
    public async Task<CharacterDto> CreateCharacter(CreateCharacterDto request)
    {
        try {
            await _uow.BeginAsync();

            var character = new Character {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
            };

            // save to the repository
            await _uow.CharacterRepository.Create(character);
            // save and commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return the character
            return _genService.MapCharacter(character, new List<Dialogue>());
        }
        catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error creating character");
            throw;
        }
    }

    // update a character
    public async Task<CharacterDto> UpdateCharacter(UpdateCharacterDto request)
    {
        try {
            await _uow.BeginAsync();

            var character = await _uow.CharacterRepository.GetById(request.Id);
            if (character == null) {
                _logger.LogWarning("[Storyservice] Character with id {CharacterId} not found", request.Id);
                throw new Exception("Character not found");
            }

            character.Name = request.Name;
            character.Description = request.Description;
            character.ImageUrl = request.ImageUrl;

            await _uow.CharacterRepository.Update(character);
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            var dialogues = await _uow.CharacterRepository.GetAllDialoguesOfCharacter(character.Id);
            return _genService.MapCharacter(character, dialogues);
        } catch (Exception ex) {
            await _uow.RollBackAsync();
            _logger.LogError(ex, "[Storyservice] Error updating character with id {CharacterId}", request.Id);
            throw;
        }
    }

}