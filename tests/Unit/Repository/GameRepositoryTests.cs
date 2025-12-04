using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Implementations;
using backend.Infrastructure.Logging;
using Moq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace tests.Unit.Repository;

public class GameRepositoryTests 
{
    // create the db context
    private static AppDbContext InMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: dbName)
			.EnableSensitiveDataLogging()
			.Options;
		return new AppDbContext(options);
    }

    // CREATE Tests
    [Fact]
    public async Task CreateGameSave_ShouldCreateGameSave()
    {
        // set up the game context
        var context = InMemoryContext(Guid.NewGuid().ToString());
        var mockLogger = new Mock<IEntityFileLogger>();
        var repo = new GameRepository(context, mockLogger.Object);
		var gameSave = new GameSave { 
            Id = 1,
            UserId = 1, 
            SaveName = "Test Save", 
            PlayerCharacterId = 1,
            CurrentStoryNodeId = 1,
            LastUpdate = DateTime.UtcNow
        };

        // create the game save in the db. 
        await repo.Create(gameSave);

        // check if the db value matches what we expect.
        var gameSaveDb = await context.GameSaves.FirstOrDefaultAsync(gs => gs.Id == gameSave.Id);

        Assert.NotNull(gameSaveDb);
        Assert.Equal("Test Save", gameSaveDb!.SaveName);
    }

	[Fact]
	public async Task CreateGameSave_ShouldThrowException_WhenGameSaveIsNull()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		await Assert.ThrowsAsync<ArgumentNullException>(() => repo.Create(null!));
	}

	// READ Tests
	[Fact]
	public async Task GetById_ShouldThrowException_WhenNotFound()
	{
    		// set up the game context
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		// get a game save that does not exist.
		await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetById(999));
	}

	[Fact]
	public async Task GetById_ShouldThrowException_WhenIdIsInvalid()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		// Try to get a game save with a non-existent ID
		await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetById(0));
	}

	[Fact]
	public async Task GetAll_ShouldReturnAllGameSaves()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		context.GameSaves.AddRange(
			new GameSave { Id = 1, UserId = 1, SaveName = "Save A", PlayerCharacterId = 1, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow },
			new GameSave { Id = 2, UserId = 1, SaveName = "Save B", PlayerCharacterId = 1, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow }
		);
		await context.SaveChangesAsync();

		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		var all = await repo.GetAll();

		Assert.Equal(2, all.Count());
	}

	[Fact]
	public async Task GetAll_ShouldReturnEmptyList_WhenNoGameSavesExist()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		var all = await repo.GetAll();

		Assert.Empty(all);
	}

	[Fact]
	public async Task GetAllByUserId_ShouldReturnUserGameSaves()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		// Add game saves for different users
		context.GameSaves.AddRange(
			new GameSave { Id = 1, UserId = 1, SaveName = "User1 Save1", PlayerCharacterId = 1, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow },
			new GameSave { Id = 2, UserId = 1, SaveName = "User1 Save2", PlayerCharacterId = 1, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow },
			new GameSave { Id = 3, UserId = 2, SaveName = "User2 Save1", PlayerCharacterId = 2, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow }
		);
		await context.SaveChangesAsync();

		var user1Saves = await repo.GetAllByUserId(1);

		Assert.Equal(2, user1Saves.Count());
		Assert.All(user1Saves, save => Assert.Equal(1, save.UserId));
	}

	[Fact]
	public async Task GetAllByUserId_ShouldReturnEmptyList_WhenUserHasNoGameSaves()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		// Add game saves for a different user
		context.GameSaves.Add(
			new GameSave { Id = 1, UserId = 2, SaveName = "User2 Save", PlayerCharacterId = 1, CurrentStoryNodeId = 1, LastUpdate = DateTime.UtcNow }
		);
		await context.SaveChangesAsync();

		var user1Saves = await repo.GetAllByUserId(1);

		Assert.Empty(user1Saves);
	}

	// UPDATE Tests
	[Fact]
	public async Task Update_ShouldModifyEntity()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var gameSave = new GameSave { 
            Id = 1,
            UserId = 1, 
            SaveName = "Old Save", 
            PlayerCharacterId = 1,
            CurrentStoryNodeId = 1,
            LastUpdate = DateTime.UtcNow
        };
		context.GameSaves.Add(gameSave);
		await context.SaveChangesAsync();

		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		gameSave.SaveName = "New Save";
		await repo.Update(gameSave);

		var fromDb = await context.GameSaves.FindAsync(gameSave.Id);
		Assert.Equal("New Save", fromDb!.SaveName);
	}

	[Fact]
	public async Task Update_ShouldThrowException_WhenGameSaveNotFound()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		var gameSave = new GameSave 
		{ 
			Id = 999, 
			UserId = 1, 
			SaveName = "Non-existent Save", 
			PlayerCharacterId = 1, 
			CurrentStoryNodeId = 1, 
			LastUpdate = DateTime.UtcNow 
		};

		await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.Update(gameSave));
	}

	[Fact]
	public async Task Update_ShouldThrowException_WhenGameSaveIsNull()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		await Assert.ThrowsAsync<ArgumentNullException>(() => repo.Update(null!));
	}

	// DELETE Tests
	[Fact]
	public async Task Delete_ShouldRemoveEntity()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var gameSave = new GameSave { 
            Id = 1,
            UserId = 1, 
            SaveName = "Test Save", 
            PlayerCharacterId = 1,
            CurrentStoryNodeId = 1,
            LastUpdate = DateTime.UtcNow
        };
		context.GameSaves.Add(gameSave);
		await context.SaveChangesAsync();

		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);
		await repo.Delete(gameSave.Id);

		var exists = await context.GameSaves.AnyAsync(gs => gs.Id == gameSave.Id);
		Assert.False(exists);
	}

	[Fact]
	public async Task Delete_ShouldThrowException_WhenGameSaveNotFound()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var mockLogger = new Mock<IEntityFileLogger>();
		var repo = new GameRepository(context, mockLogger.Object);

		await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.Delete(999));
	}
}