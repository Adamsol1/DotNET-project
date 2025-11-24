using backend.Application.Dtos;
using backend.Domain.Models;
// since couple of the functions we use live within system.threading we import that.
using System.Threading.Tasks;
using System;

/* 
GenService Interface - Infrastructure layer for common functionality
Focuses on transaction handling, entity validation, and DTO mapping
to eliminate duplication across all services.

-Ah 20.11
*/
namespace backend.Application.Interfaces.Services;

public interface IGenService 
{
    #region Transaction Wrappers
    // Transaction wrapper methods - eliminates 90% of duplication
    Task<T> Execute<T>(Func<Task<T>> operation);
    Task Execute(Func<Task> operation);
    #endregion

    #region Entity Validation
    // Common validation methods - eliminates null-check duplication
    Task<T> ValidateEntityExists<T>(int id) where T : class;
    Task<bool> CheckChoiceInNode(int choiceId, int nodeId);
    Task<bool> CheckStoryNodeExists(int nodeId);
    #endregion

    #region DTO Mapping Helpers
    // DTO mapping methods - eliminates mapping duplication
    StoryNodeDto MapStoryNode(StoryNode storyNode, IEnumerable<Dialogue> dialogues, IEnumerable<Choice> choices);
    ChoiceDto MapChoice(Choice choice);
    DialogueDto MapDialogue(Dialogue dialogue);
    CharacterDto MapCharacter(Character character, IEnumerable<Dialogue> dialogues);
    GameSaveDto MapGameSave(GameSave gameSave);
    
    #endregion
}


