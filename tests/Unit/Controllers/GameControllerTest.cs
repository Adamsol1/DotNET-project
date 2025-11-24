using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Application.Dtos;
using backend.Application.Interfaces.Services;
using backend.Controllers;
using backend.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace tests.Unit.Controllers
{
    public class GameControllerTests
    {
        private readonly Mock<IGameService> _gameService = new();
        private readonly Mock<IPlayerService> _playerService = new();

        private GameController CreateController()
        {
            return new GameController(_gameService.Object, _playerService.Object);
        }

        [Fact]
        public async Task Start_ReturnsOk_WhenGameCreated()
        {
            // Arrange
            var request = new StartGameRequestDto { UserId = 1, SaveName = "Test Save" };
            var gameSave = new GameSave 
            { 
                Id = 1, 
                UserId = 1, 
                SaveName = "Test Save", 
                PlayerCharacterId = 1,
                CurrentStoryNodeId = 1,
                LastUpdate = DateTime.UtcNow
            };
            
            _gameService.Setup(s => s.CreateGame(request.UserId, request.SaveName))
                       .ReturnsAsync(gameSave);

            var controller = CreateController();

            // Act
            var result = await controller.Start(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGameSave = Assert.IsType<GameSave>(okResult.Value);
            Assert.Equal(1, returnedGameSave.Id);
            Assert.Equal("Test Save", returnedGameSave.SaveName);
        }

        [Fact]
        public async Task Start_ReturnsBadRequest_WhenGameCreationFails()
        {
            // Arrange
            var request = new StartGameRequestDto { UserId = 1, SaveName = "Test Save" };
            _gameService.Setup(s => s.CreateGame(request.UserId, request.SaveName))
                       .ThrowsAsync(new Exception("Test exception"));

            var controller = CreateController();

            // Act
            var result = await controller.Start(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed to start game", badRequestResult.Value);
        }

        [Fact]
        public async Task LoadGame_ReturnsOk_WhenGameFound()
        {
            // Arrange
            var saveId = 1;
            var gameSave = new GameSave 
            { 
                Id = saveId, 
                UserId = 1, 
                SaveName = "Test Save", 
                PlayerCharacterId = 1,
                CurrentStoryNodeId = 1,
                LastUpdate = DateTime.UtcNow
            };
            
            _gameService.Setup(s => s.GetGameSave(saveId))
                       .ReturnsAsync(gameSave);

            var controller = CreateController();

            // Act
            var result = await controller.LoadGame(saveId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGameSave = Assert.IsType<GameSave>(okResult.Value);
            Assert.Equal(saveId, returnedGameSave.Id);
        }

        [Fact]
        public async Task LoadGame_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            var saveId = 1;
            _gameService.Setup(s => s.GetGameSave(saveId))
                       .ThrowsAsync(new Exception("Test exception"));

            var controller = CreateController();

            // Act
            var result = await controller.LoadGame(saveId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Failed to load game", badRequestResult.Value);
        }

        [Fact]
        public async Task MakeChoice_ReturnsOk_WhenChoiceMade()
        {
            // Arrange
            var request = new MakeChoiceRequestDto { SaveId = 1, ChoiceId = 1 };
            var gameSave = new GameSave 
            { 
                Id = 1, 
                UserId = 1, 
                SaveName = "Test Save", 
                PlayerCharacterId = 1,
                CurrentStoryNodeId = 2,
                LastUpdate = DateTime.UtcNow
            };
            
            _gameService.Setup(s => s.MakeChoice(request.SaveId, request.ChoiceId))
                       .ReturnsAsync(gameSave);

            var controller = CreateController();

            // Act
            var result = await controller.MakeChoice(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGameSave = Assert.IsType<GameSave>(okResult.Value);
            Assert.Equal(2, returnedGameSave.CurrentStoryNodeId);
        }

        [Fact]
        public async Task GetUserSaves_ReturnsOk_WithUserSaves()
        {
            // Arrange
            var userId = 1;
            var gameSaves = new List<GameSave>
            {
                new GameSave { Id = 1, UserId = userId, SaveName = "Save 1", PlayerCharacterId = 1, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow },
                new GameSave { Id = 2, UserId = userId, SaveName = "Save 2", PlayerCharacterId = 1, CurrentStoryNodeId = 2, LastUpdate = DateTime.UtcNow }
            };
            
            _gameService.Setup(s => s.GetUserGameSaves(userId))
                       .ReturnsAsync(gameSaves);

            var controller = CreateController();

            // Act
            var result = await controller.GetUserSaves(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedSaves = Assert.IsAssignableFrom<IEnumerable<GameSave>>(okResult.Value);
            Assert.Equal(2, returnedSaves.Count());
        }

        [Fact]
        public async Task DeleteGameSave_ReturnsOk_WhenDeleted()
        {
            // Arrange
            var saveId = 1;
            _gameService.Setup(s => s.DeleteGameSave(saveId))
                       .ReturnsAsync(true);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteGameSave(saveId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = okResult.Value;
            Assert.NotNull(response);
        }

        [Fact]
        public async Task DeleteGameSave_ReturnsNotFound_WhenDeleteFails()
        {
            // Arrange
            var saveId = 1;
            _gameService.Setup(s => s.DeleteGameSave(saveId))
                       .ReturnsAsync(false);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteGameSave(saveId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal("Failed to delete game save", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateGameSave_ReturnsOk_WhenUpdated()
        {
            // Arrange
            var saveId = 1;
            var request = new UpdateGameSaveRequest { SaveId = saveId, CurrentStoryNodeId = 2 };
            var gameSave = new GameSave 
            { 
                Id = saveId, 
                UserId = 1, 
                SaveName = "Test Save", 
                PlayerCharacterId = 1,
                CurrentStoryNodeId = 2,
                LastUpdate = DateTime.UtcNow
            };
            
            _gameService.Setup(s => s.UpdateGameSave(saveId, request.CurrentStoryNodeId))
                       .ReturnsAsync(gameSave);

            var controller = CreateController();

            // Act
            var result = await controller.UpdateGameSave(saveId, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGameSave = Assert.IsType<GameSave>(okResult.Value);
            Assert.Equal(2, returnedGameSave.CurrentStoryNodeId);
        }
    }
}
