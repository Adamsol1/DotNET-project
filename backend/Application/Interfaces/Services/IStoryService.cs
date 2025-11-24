using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Application.Dtos;

// Story Service Interface

namespace backend.Application.Interfaces.Services;

public interface IStoryService
{
    /*
    Since we would need a story composed of StoryNodes, Dialogues, Choices and characters,
    I though that it was best to combine all those stuff into a single service.
    that will be used in GameService.

    Ahmed
    
    */

    //-- Story Node Methods --
    // get Story Its node Id
    Task<StoryNodeDto> GetStoryNodeById(int id);

    // Get a list of all story nodes
    // since we dont want to change the story nodes, we can use IReadOnlyList.
    // we can add a dedicated method for this if we want to.
    Task<IEnumerable<StoryNodeDto>> GetAllStoryNodes();

    //create a StoryNode
    Task<StoryNodeDto> CreateStoryNode(CreateStoryNodeDto request);

    // update a StoryNode
    Task<StoryNodeDto> UpdateStoryNode(UpdateStoryNodeDto request);

    // delete a StoryNode
    Task<bool> DeleteStoryNode(int id);

    //-- Dialogue Methods --
    // get a dialogues in the story node
    Task<IEnumerable<DialogueDto>> GetDialoguesInStoryNode(int storyNodeId);

    // create a Dialogue
    Task<DialogueDto> CreateDialogue(CreateDialogueDto request);

    // update a Dialogue
    Task<DialogueDto> UpdateDialogue(UpdateDialogueDto request);

    // delete a Dialogue
    Task<bool> DeleteDialogue(int id);

    //-- Choice Methods --
    // get a choices in the story node
    Task<IEnumerable<ChoiceDto>> GetChoicesInStoryNode(int storyNodeId);

    // create a Choice
    Task<ChoiceDto> CreateChoice(CreateChoiceDto request);

    // update a Choice
    Task<ChoiceDto> UpdateChoice(UpdateChoiceDto request);

    // delete a Choice
    Task<bool> DeleteChoice(int id);

    //-- Character Methods --
    //get a single character
    Task<CharacterDto> GetCharacterById(int id);

    // get a list of all characters
    Task<IEnumerable<CharacterDto>> GetAllCharacters();

    // get a characters in the story node
    Task<IEnumerable<CharacterDto>> GetCharactersInStoryNode(int storyNodeId);

    // create a Character
    Task<CharacterDto> CreateCharacter(CreateCharacterDto request);

    // update a Character
    Task<CharacterDto> UpdateCharacter(UpdateCharacterDto request);
    
    // delete a Character
    //Task<bool> DeleteCharacter(int id);

}