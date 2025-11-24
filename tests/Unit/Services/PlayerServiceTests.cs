using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.Application;
using backend.Application.Dtos;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace tests.Unit.Services
{
    public class PlayerServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IPlayerCharacterRepository> _playerRepo = new();
        private readonly Mock<ILogger<PlayerService>> _logger = new();

        private PlayerService CreateService()
        {
            _uow.Setup(u => u.PlayerCharacterRepository).Returns(_playerRepo.Object);
            
            // No-op standard for transaksjoner
            _uow.Setup(x => x.BeginAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
            _uow.Setup(x => x.RollBackAsync()).Returns(Task.CompletedTask);

            return new PlayerService(_uow.Object, _logger.Object);
        }

        private static PlayerCharacter Pc(int id = 1, int health = 100) =>
            new PlayerCharacter { Id = id, Name = "Hero", UserId = 1, CurrentStoryNodeId = 10, Health = health };

        [Fact]
        public async Task DeleteCharacter_Success()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(11))
                      .ReturnsAsync(Pc(id: 11));
            _playerRepo.Setup(r => r.Delete(11))
                      .ReturnsAsync(Pc(id: 11));

            // Act
            var ok = await sut.DeleteCharacter(11);

            // Assert
            Assert.True(ok);
            _playerRepo.Verify(r => r.Delete(11), Times.Once);
            _uow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCharacter_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(999))
                      .ReturnsAsync((PlayerCharacter?)null);

            // Act
            var ok = await sut.DeleteCharacter(999);

            // Assert
            Assert.False(ok);
            _playerRepo.Verify(r => r.Delete(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(50, 30, 30)]
        [InlineData(50, -10, 0)]
        public async Task SetHealth_Success(int initial, int newHealth, int expected)
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(1)).ReturnsAsync(Pc(id: 1, health: initial));
            _playerRepo.Setup(r => r.Update(It.IsAny<PlayerCharacter>()))
                .ReturnsAsync((PlayerCharacter pc) => pc);

            // Act
            var result = await sut.SetHealth(1, newHealth);

            // Assert
            Assert.Equal(expected, result);
            _playerRepo.Verify(r => r.Update(It.Is<PlayerCharacter>(p => p.Health == expected)), Times.Once);
        }

        [Fact]
        public async Task SetHealth_ThrowsException_WhenPlayerNotFound()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(999))
                      .ReturnsAsync((PlayerCharacter?)null);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => sut.SetHealth(999, 50));
            Assert.Contains("Player character not found", ex.Message);
        }

        [Theory]
        [InlineData(50, 10, 60)]
        [InlineData(3, -10, 0)]
        public async Task ModifyHealth_Success(int initial, int delta, int expected)
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(2)).ReturnsAsync(Pc(id: 2, health: initial));
            _playerRepo.Setup(r => r.Update(It.IsAny<PlayerCharacter>()))
                .ReturnsAsync((PlayerCharacter pc) => pc);

            // Act
            var result = await sut.ModifyHealth(2, delta);

            // Assert
            Assert.Equal(expected, result);
            _playerRepo.Verify(r => r.Update(It.Is<PlayerCharacter>(p => p.Health == expected)), Times.Once);
            _uow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetHealth_Success()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(1))
                      .ReturnsAsync(Pc(id: 1, health: 75));

            // Act
            var result = await sut.GetHealth(1);

            // Assert
            Assert.Equal(75, result);
        }

        [Fact]
        public async Task GetHealth_ThrowsException_WhenPlayerNotFound()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(999))
                      .ReturnsAsync((PlayerCharacter?)null);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => sut.GetHealth(999));
            Assert.Contains("Player character not found", ex.Message);
        }

        [Theory]
        [InlineData(50, true)]
        [InlineData(0, false)]
        [InlineData(-10, false)]
        public async Task IsAlive_Success(int health, bool expected)
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(1))
                      .ReturnsAsync(Pc(id: 1, health: health));

            // Act
            var result = await sut.IsAlive(1);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Heal_Success()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(1)).ReturnsAsync(Pc(id: 1, health: 30));
            _playerRepo.Setup(r => r.Update(It.IsAny<PlayerCharacter>()))
                .ReturnsAsync((PlayerCharacter pc) => pc);

            // Act
            var result = await sut.Heal(1, 20);

            // Assert
            Assert.Equal(50, result);
        }

        [Fact]
        public async Task Damage_Success()
        {
            // Arrange
            var sut = CreateService();
            _playerRepo.Setup(r => r.GetById(1)).ReturnsAsync(Pc(id: 1, health: 50));
            _playerRepo.Setup(r => r.Update(It.IsAny<PlayerCharacter>()))
                .ReturnsAsync((PlayerCharacter pc) => pc);

            // Act
            var result = await sut.Damage(1, 20);

            // Assert
            Assert.Equal(30, result);
        }
    }
}