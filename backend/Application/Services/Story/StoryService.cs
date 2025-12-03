using backend.Application.Dtos.Story;
using backend.Application.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;

namespace backend.Application.Services.Story;

/// <summary>
/// Service for managing story nodes, dialogues, choices, and characters including CRUD operations and retrieval.
/// </summary>  
public class StoryService : IStoryService
{
    // dependencies
    private readonly IUnitOfWork _uow;
    private readonly IGenService _genService;
    private readonly IEntityFileLogger _entityLogger;

    // constructor
    public StoryService(IUnitOfWork uow, IGenService genService, IEntityFileLogger entityLogger)
    {
        _entityLogger = entityLogger;
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

            // map and return
            return _genService.MapStoryNode(storyNode, dialogues, choices);
        }
        catch
        {
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
        } catch {
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
                throw new Exception("Choices not found");
            }
            // return the choices to DTO object
            return choices.Select(_genService.MapChoice);
        }
        catch {
            await _uow.RollBackAsync();
            throw;
        }
    }
}