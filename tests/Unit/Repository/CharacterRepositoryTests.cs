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

public class CharacterRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CharacterRepository _repository;

    public CharacterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new CharacterRepository(_context);
    }

    [Fact]
    public async Task GetCharacterByName_ShouldReturnCharacter()
    {
        // create the context the tests needs.
        var character = new Character { Id = 1, Name = "Ryan", Description = "Main story character" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // get the caracter from the repository
        var result = await _repository.GetCharacterByName("Ryan");

        // assesmets of the tests are true or false.
        Assert.NotNull(result);
        Assert.Equal("Ryan", result.Name);
        Assert.Equal("Main story character", result.Description);
    }

    [Fact]
    public async Task GetCharacterByName_ShouldReturnNull()
    {
        // start
        var character = new Character { Id = 1, Name = "Ryan", Description = "Main story character" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // try to get the false character
        var result = await _repository.GetCharacterByName("Batman");

        // assesmets of the tests are true or false.
        Assert.Null(result);
    }

    [Fact]
    public async Task GetCharacterNameById_ShouldGetName()
    {
        // staging the test
        var character = new Character { Id = 1, Name = "Ryan", Description = "Main game character" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCharacterNameById(1);

        // Assert
        Assert.Equal("Ryan", result);
    }

    [Fact]
    public async Task GetAllCharactersWithName_ShouldAllCharacters()
    {
        // create the tests objects.
        var characters = new List<Character>
        {
            new Character { Id = 1, Name = "Ryan", Description = "not really important" },
            new Character { Id = 2, Name = "Ryan", Description = "ogaboga" },
            new Character { Id = 3, Name = "Bob", Description = "bobby" }
        };
        await _context.Characters.AddRangeAsync(characters);
        await _context.SaveChangesAsync();

        // get the result from the repository
        var result = await _repository.GetAllCharactersWithName("Ryan");

        // assesment of the test
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal("Hero", c.Name));
    }

    [Fact]
    public async Task GetCharacterDescription_ShouldReturn()
    {
        // create the object.
        var character = new Character { Id = 1, Name = "Ryan", Description = "someones son" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // get the object
        var result = await _repository.GetCharacterDescription(1);

        // check if the object is what we expect
        Assert.Equal("someones son", result);
    }

    [Fact]
    public async Task GetCharacterImage_ShouldReturnImage()
    {
        // Arrange
        var character = new Character { Id = 1, Name = "Ryan", Description = "RyanRyan", ImageUrl = "ryan.png" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCharacterImageUrl(1);

        // Assert
        Assert.Equal("ryan.png", result);
    }

    [Fact]
    public async Task GetAllDialoguesOfCharacter_ShouldReturnDialogues()
    {
        // create the objects and save to the db
        var character = new Character { Id = 1, Name = "Ryan", Description = "uhm some umh" };
        
        var dialogues = new List<Dialogue>
        {
            new Dialogue { Id = 1, CharacterId = 1, Text = "Hello Welcome to the last fronttier," },
            new Dialogue { Id = 2, CharacterId = 1, Text = "What is your name again?" }
        };
        
        await _context.Characters.AddAsync(character);
        await _context.Dialogues.AddRangeAsync(dialogues);
        await _context.SaveChangesAsync();

        // get the object
        var result = await _repository.GetAllDialoguesOfCharacter(1);

        // asses if the the test if right.
        Assert.Equal(2, result.Count());
        Assert.All(result, d => Assert.Equal(1, d.CharacterId));
    }

    [Fact]
    public async Task GetAllDialoguesOfCharacter_ShouldReturnEmptyList()
    {
        // object creation
        var character = new Character { Id = 1, Name = "Humpty", Description = "Someone from alice in the wonderland" };
        await _context.Characters.AddAsync(character);
        await _context.SaveChangesAsync();

        // get the object
        var result = await _repository.GetAllDialoguesOfCharacter(1);

        // check if its what we expect
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
