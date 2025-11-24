using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Domain.Models;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories;
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

    [Fact]
    public async Task CreateGameSave_ShouldCreateGameSave()
    {
        // set up the game context
        var context = InMemoryContext(Guid.NewGuid().ToString());
        var repo = new GameRepository(context);
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
	public async Task GetById_ShouldThrowException_WhenNotFound()
	{
    		// set up the game context
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var repo = new GameRepository(context);

		// get a game save that does not exist.
		await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.GetById(999));
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

		var repo = new GameRepository(context);

		var all = await repo.GetAll();

		Assert.Equal(2, all.Count());
	}

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

		var repo = new GameRepository(context);

		gameSave.SaveName = "New Save";
		await repo.Update(gameSave);

		var fromDb = await context.GameSaves.FindAsync(gameSave.Id);
		Assert.Equal("New Save", fromDb!.SaveName);
	}

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

		var repo = new GameRepository(context);
		await repo.Delete(gameSave.Id);

		var exists = await context.GameSaves.AnyAsync(gs => gs.Id == gameSave.Id);
		Assert.False(exists);
	}

	[Fact]
	public async Task GetAllByUserId_ShouldReturnUserGameSaves()
	{
		var context = InMemoryContext(Guid.NewGuid().ToString());
		var repo = new GameRepository(context);

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
}