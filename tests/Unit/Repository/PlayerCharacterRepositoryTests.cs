using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories;
using backend.Domain.Models;
using Xunit;

namespace tests.Unit.Repository;

public class PlayerCharacterRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PlayerCharacterRepository _repository;

    public PlayerCharacterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new PlayerCharacterRepository(_context);
    }

    [Fact]
    public async Task GetHealthByIdAsync_ShouldReturnHealth()
    {
        // create the object
        var playerCharacter = new PlayerCharacter 
        { 
            Id = 1, 
            Name = "Ryan", 
            Health = 100, 
            UserId = 1, 
            CurrentStoryNodeId = 1 
        };
        await _context.Characters.AddAsync(playerCharacter);
        await _context.SaveChangesAsync();

        // get the object from the repository
        var result = await _repository.GetHealthByIdAsync(1);

        // check if the result is what we expect
        Assert.Equal(100, result);
    }

    [Fact]
    public async Task GetAllByUserId_ShouldReturnAllPlayerCharacters()
    {
        // create the object
        var playerCharacters = new List<PlayerCharacter>
        {
            new PlayerCharacter { Id = 1, Name = "Ryan", Health = 100, UserId = 1, CurrentStoryNodeId = 1 },
            new PlayerCharacter { Id = 2, Name = "Ryan2", Health = 80, UserId = 1, CurrentStoryNodeId = 2 },
            new PlayerCharacter { Id = 3, Name = "Ryan3", Health = 90, UserId = 2, CurrentStoryNodeId = 1 }
        };
        await _context.Characters.AddRangeAsync(playerCharacters);
        await _context.SaveChangesAsync();

        // get the object from the repository
        var result = await _repository.GetAllByUserId(1);

        // check if the result is what we expect
        Assert.Equal(2, result.Count());
        Assert.All(result, pc => Assert.Equal(1, pc.UserId));
    }

    [Fact]
    public async Task GetAllByUserId_ShouldReturnEmptyList()
    {
        // create the object
        var playerCharacter = new PlayerCharacter 
        { 
            Id = 1, 
            Name = "Ryan", 
            Health = 100, 
            UserId = 1, 
            CurrentStoryNodeId = 1 
        };
        await _context.Characters.AddAsync(playerCharacter);
        await _context.SaveChangesAsync();

        // get the object from the repository
        var result = await _repository.GetAllByUserId(999);

        // asses if the result is empty
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByUserId_ShouldReturnFirstPlayerCharacter()
    {
        // create the object
        var playerCharacters = new List<PlayerCharacter>
        {
            new PlayerCharacter { Id = 1, Name = "Ryan", Health = 100, UserId = 1, CurrentStoryNodeId = 1 },
            new PlayerCharacter { Id = 2, Name = "Ryan2", Health = 80, UserId = 1, CurrentStoryNodeId = 2 }
        };
        await _context.Characters.AddRangeAsync(playerCharacters);
        await _context.SaveChangesAsync();

        // get the object from the repository
        var result = await _repository.GetByUserId(1);

        // asses if the result is what we want
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("Ryan", result.Name);
    }

    [Fact]
    public async Task GetByUserId_ShouldReturnNull()
    {
        // create the object
        var playerCharacter = new PlayerCharacter 
        { 
            Id = 1, 
            Name = "Ryan", 
            Health = 100, 
            UserId = 1, 
            CurrentStoryNodeId = 1 
        };
        await _context.Characters.AddAsync(playerCharacter);
        await _context.SaveChangesAsync();

        // get the object from the repository
        var result = await _repository.GetByUserId(99999);

        // assesment if the result is true
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserId_ShouldUseGenericMethod()
    {
        // Arrangecreae the object
        var playerCharacter = new PlayerCharacter 
        { 
            Id = 1, 
            Name = "Ryan", 
            Health = 100, 
            UserId = 1, 
            CurrentStoryNodeId = 1 
        };
        await _context.Characters.AddAsync(playerCharacter);
        await _context.SaveChangesAsync();

        // get the result
        var result = await _repository.GetByUserId(1);

        // check if the result is what we expect
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("Ryan", result.Name);
    }

    [Fact]
    public async Task GetAllByUserId_ShouldUseGenericMethod()
    {
        // ccreate the object
        var playerCharacters = new List<PlayerCharacter>
        {
            new PlayerCharacter { Id = 1, Name = "Ryan", Health = 100, UserId = 1, CurrentStoryNodeId = 1 },
            new PlayerCharacter { Id = 2, Name = "Ryan2", Health = 80, UserId = 1, CurrentStoryNodeId = 2 }
        };
        await _context.Characters.AddRangeAsync(playerCharacters);
        await _context.SaveChangesAsync();

        // get the result from the database
        var result = await _repository.GetAllByUserId(1);

        // Check if the test are matching what we expect
        Assert.Equal(2, result.Count());
        Assert.All(result, pc => Assert.Equal(1, pc.UserId));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
