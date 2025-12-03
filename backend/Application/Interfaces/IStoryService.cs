using backend.Application.Dtos.Story;

// Story Service Interface

namespace backend.Application.Interfaces;


/// <summary>
/// Interface for managing:
/// - Story Nodes
/// - Dialogues
/// - Choices
/// - Characters
/// Used by GameService to handle story related commands. 
/// This has been created to handle all story related operations in a single service that will be used in GameService.
/// </summary>

public interface IStoryService
{

    //-- Story Node Methods --
    // get Story Its node Id
    Task<StoryNodeDto> GetStoryNodeById(int id);

    //-- Dialogue Methods --
    // get a dialogues in the story node
    Task<IEnumerable<DialogueDto>> GetDialoguesInStoryNode(int storyNodeId);


    //-- Choice Methods --
    // get a choices in the story node
    Task<IEnumerable<ChoiceDto>> GetChoicesInStoryNode(int storyNodeId);



}