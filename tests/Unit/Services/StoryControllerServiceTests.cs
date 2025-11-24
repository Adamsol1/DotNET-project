using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Application;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using backend.Domain.Models;

namespace tests;

public class StoryControllerServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGenService> _mockGenService;
    private readonly Mock<IStoryService> _mockStoryService;
    private readonly Mock<ILogger<StoryControllerService>> _mockLogger;
    private readonly StoryControllerService _service;

    public StoryControllerServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockGenService = new Mock<IGenService>();
        _mockStoryService = new Mock<IStoryService>();
        _mockLogger = new Mock<ILogger<StoryControllerService>>();
        
        _service = new StoryControllerService(
            _mockUnitOfWork.Object,
            _mockGenService.Object,
            _mockStoryService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetCurrentNode_ShouldReturnStoryNode()
    {
        // Arrange
        var saveId = 1;
        var gameSave = new GameSave 
        { 
            Id = saveId, 
            CurrentStoryNodeId = 1,
            VisitedNodeIds = "[]",
            LastChoiceId = null,
            CurrentDialogueIndex = 0
        };
        var storyNodeDto = new StoryNodeDto { Id = 1, Title = "Test Node" };

        _mockGenService.Setup(x => x.Execute(It.IsAny<Func<Task<StoryNodeDto>>>()))
            .ReturnsAsync(storyNodeDto);
        _mockGenService.Setup(x => x.ValidateEntityExists<GameSave>(saveId))
            .ReturnsAsync(gameSave);
        _mockStoryService.Setup(x => x.GetStoryNodeById(1))
            .ReturnsAsync(storyNodeDto);

        // Act
        var result = await _service.GetCurrentNode(saveId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Node", result.Title);
    }

    [Fact]
    public async Task GoBack_WithNoLastChoice_ShouldReturnNull()
    {
        // Arrange
        var saveId = 1;
        var gameSave = new GameSave 
        { 
            Id = saveId, 
            CurrentStoryNodeId = 1,
            LastChoiceId = null // No last choice
        };

        _mockGenService.Setup(x => x.Execute(It.IsAny<Func<Task<StoryNodeDto?>>>()))
            .ReturnsAsync((StoryNodeDto?)null);
        _mockGenService.Setup(x => x.ValidateEntityExists<GameSave>(saveId))
            .ReturnsAsync(gameSave);

        // Act
        var result = await _service.GoBack(saveId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task MakeChoice_ShouldStoreChoiceId()
    {
        // Arrange
        var saveId = 1;
        var choiceId = 1;
        var gameSave = new GameSave 
        { 
            Id = saveId, 
            CurrentStoryNodeId = 1,
            LastChoiceId = null
        };
        var choice = new Choice { Id = choiceId, StoryNodeId = 1, NextStoryNodeId = 2 };
        var storyNodeDto = new StoryNodeDto { Id = 2, Title = "Next Node" };

        _mockGenService.Setup(x => x.Execute(It.IsAny<Func<Task<StoryNodeDto>>>()))
            .ReturnsAsync(storyNodeDto);
        _mockGenService.Setup(x => x.ValidateEntityExists<GameSave>(saveId))
            .ReturnsAsync(gameSave);
        _mockGenService.Setup(x => x.CheckChoiceInNode(choiceId, 1))
            .ReturnsAsync(true);
        _mockGenService.Setup(x => x.ValidateEntityExists<Choice>(choiceId))
            .ReturnsAsync(choice);

        // Act
        var result = await _service.MakeChoice(saveId, choiceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }
}
