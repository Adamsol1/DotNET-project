using backend.Application.Dtos.Game;
using backend.Application.Dtos.Story;
using backend.Domain.Models;



/// <summary>
/// GenService Interface - Infrastructure layer for common functionality
/// Focuses on transaction handling, entity validation, and DTO mapping
/// to eliminate duplication across all services.
/// </summary>

namespace backend.Application.Interfaces;

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


