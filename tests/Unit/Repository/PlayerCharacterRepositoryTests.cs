using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Implementations;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using Moq;
using Xunit;

namespace tests.Unit.Repository;

public class PlayerCharacterRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PlayerCharacterRepository _repository;
    private readonly Mock<IEntityFileLogger> _mockLogger;

    public PlayerCharacterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<IEntityFileLogger>();
        _repository = new PlayerCharacterRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetHealthByIdAsync_ShouldReturnHealth()
    {
        // create the object
        var playerCharacter = new PlayerCharacter 
        { 
            Id = 1, 
            Name = "Ryan", 
            Health = 100
        };
        await _context.Characters.AddAsync(playerCharacter);
        await _context.SaveChangesAsync();

        // get the object from the repository
        var result = await _repository.GetHealthByIdAsync(1);

        // check if the result is what we expect
        Assert.Equal(100, result);
    }

    [Fact]
    public async Task GetHealthByIdAsync_ShouldReturnZero_WhenNotFound()
    {
        // get the object from the repository for non-existent ID
        var result = await _repository.GetHealthByIdAsync(999);

        // check if the result is zero (default for int)
        Assert.Equal(0, result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
