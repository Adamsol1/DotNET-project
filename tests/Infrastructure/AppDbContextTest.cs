using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Domain.Models;
using Xunit;
using System.Linq;
using System;

namespace tests;

public class AppDbContextTests
{
    [Fact]
    public void Test_AddAndRetrieveCharacter()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();

            var character = new Character
            {
                Name = "Hero",
                Description = "The main protagonist."
            };
            context.Characters.Add(character);
            context.SaveChanges();
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var character = context.Characters.FirstOrDefault(c => c.Name == "Hero");
            Assert.NotNull(character);
            Assert.Equal("The main protagonist.", character!.Description);
        }
    }

    [Fact]
    public void Test_AddAndRetrieveStoryNode()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();

            var storyNode = new StoryNode
            {
                Title = "Beginning",
                Description = "This is the start of the adventure."
            };
            context.StoryNodes.Add(storyNode);
            context.SaveChanges();
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var storyNode = context.StoryNodes.FirstOrDefault(sn => sn.Title == "Beginning");
            Assert.NotNull(storyNode);
            Assert.Equal("This is the start of the adventure.", storyNode!.Description);
        }
    }

    [Fact]
    public void Test_AddAndRetrieveChoice(){
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();

            var choice = new Choice
            {
                Text = "Go left",
                NextStoryNodeId = 2
            };
            context.Choices.Add(choice);
            context.SaveChanges();
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var choice = context.Choices.FirstOrDefault(c => c.Text == "Go left");
            Assert.NotNull(choice);
            Assert.Equal(2, choice!.NextStoryNodeId);
        }
    }

    [Fact]
    public void Test_AddAndRetrieveDialogue(){
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();

            var dialogue = new Dialogue
            {
                Text = "Welcome to the adventure!",
                CharacterId = 1,
                StoryNodeId = 1
            };
            context.Dialogues.Add(dialogue);
            context.SaveChanges();
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var dialogue = context.Dialogues.FirstOrDefault(d => d.Text == "Welcome to the adventure!");
            Assert.NotNull(dialogue);
            Assert.Equal(1, dialogue!.CharacterId);
            Assert.Equal(1, dialogue.StoryNodeId);
        }
    }

    [Fact]
    public void Test_Relationships()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act
        using (var context = new AppDbContext(options))
        {
            context.Database.EnsureCreated();

            var character = new Character { Name = "Hero", Description = "The main protagonist." };
            var storyNode = new StoryNode { Title = "Beginning", Description = "This is the start of the adventure." };
            context.Characters.Add(character);
            context.StoryNodes.Add(storyNode);
            context.SaveChanges();

            var dialogue = new Dialogue { Text = "Welcome to the adventure!", CharacterId = character.Id, StoryNodeId = storyNode.Id };
            var choice = new Choice { Text = "Go left", NextStoryNodeId = storyNode.Id + 1, StoryNodeId = storyNode.Id };
            context.Dialogues.Add(dialogue);
            context.Choices.Add(choice);
            context.SaveChanges();
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var dialogue = context.Dialogues.Include(d => d.Character).Include(d => d.StoryNode).FirstOrDefault(d => d.Text == "Welcome to the adventure!");
            Assert.NotNull(dialogue);
            Assert.NotNull(dialogue!.Character);
            Assert.Equal("Hero", dialogue.Character!.Name);
            Assert.NotNull(dialogue.StoryNode);
            Assert.Equal("Beginning", dialogue.StoryNode!.Title);

            var choice = context.Choices.Include(c => c.StoryNode).FirstOrDefault(c => c.Text == "Go left");
            Assert.NotNull(choice);
            Assert.NotNull(choice!.StoryNode);
            Assert.Equal("Beginning", choice.StoryNode!.Title);
        }
    }
}
