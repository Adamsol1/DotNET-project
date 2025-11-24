using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;
using backend.Domain.Models;
using backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace tests
{
    public class DbSeederTests
    {
        private DbContextOptions<AppDbContext> GetInMemoryDbOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task SeedAsync_ShouldSeedDatabase_WhenDatabaseIsEmpty_TempValues()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            // Use a fresh context
            await using var context = new AppDbContext(options);

            // Create minimal JSON for testing (can be extended)
            Directory.CreateDirectory("wwwroot/seedData");

            await File.WriteAllTextAsync("wwwroot/seedData/playercharacters.json",
                JsonSerializer.Serialize(new List<PlayerCharacter>
                {
                    new() { Id = 1, Name = "Hero", Health = 100 }
                }));

            await File.WriteAllTextAsync("wwwroot/seedData/characters.json",
                JsonSerializer.Serialize(new List<Character>
                {
                    new() { Name = "Villager", Description = "Friendly NPC" }
                }));

            await File.WriteAllTextAsync("wwwroot/seedData/storynodes.json",
                JsonSerializer.Serialize(new List<StoryNode>
                {
                    new() { Title = "Start", Description = "Beginning of the story" }
                }));

            await File.WriteAllTextAsync("wwwroot/seedData/dialogues.json",
                JsonSerializer.Serialize(new List<Dialogue>
                {
                    new() { Text = "Hello!", CharacterId = 1, StoryNodeId = 1, Order = 1 }
                }));

            await File.WriteAllTextAsync("wwwroot/seedData/choices.json",
                JsonSerializer.Serialize(new List<Choice>
                {
                    new() { Text = "Go left", StoryNodeId = 1, NextStoryNodeId = 2 }
                }));
                
            // Act
            await DbSeeder.SeedAsync(context);

            // Assert
            Assert.True(await context.PlayerCharacters.AnyAsync());
            Assert.True(await context.Characters.AnyAsync());
            Assert.True(await context.StoryNodes.AnyAsync());
            Assert.True(await context.Dialogues.AnyAsync());
            Assert.True(await context.Choices.AnyAsync());

            var player = await context.PlayerCharacters.FirstAsync();
            Assert.Equal("Hero", player.Name);
            Assert.Equal(100, player.Health);

            var character = await context.Characters.FirstAsync();
            Assert.Equal("Villager", character.Name);

            var storyNode = await context.StoryNodes.FirstAsync();
            Assert.Equal("Start", storyNode.Title);

            var dialogue = await context.Dialogues.FirstAsync();
            Assert.Equal("Hello!", dialogue.Text);

            var choice = await context.Choices.FirstAsync();
            Assert.Equal("Go left", choice.Text);
        }

        [Fact]
        public async Task SeedAsync_ShouldSeedDatabase_WhenDatabaseIsEmpty()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            using (var context = new AppDbContext(options))
            {
                // Act
                await DbSeeder.SeedAsync(context);

                // Assert
                Assert.True(await context.Characters.AnyAsync(), "Characters should be seeded.");
                Assert.True(await context.PlayerCharacters.AnyAsync(), "PlayerCharacters should be seeded.");
                Assert.True(await context.StoryNodes.AnyAsync(), "StoryNodes should be seeded.");
                Assert.True(await context.Choices.AnyAsync(), "Choices should be seeded.");
                Assert.True(await context.Dialogues.AnyAsync(), "Dialogues should be seeded.");
            }
        }

        [Fact]
        public async Task SeedAsync_ShouldNotReseedDatabase_WhenDatabaseIsNotEmpty()
        {
            // Arrange
            var options = GetInMemoryDbOptions();

            using (var context = new AppDbContext(options))
            {
                // Pre-seed the database
                context.Characters.Add(new Character { Name = "Test Character" });
                await context.SaveChangesAsync();

                // Act
                await DbSeeder.SeedAsync(context);

                // Assert
                var characterCount = await context.Characters.CountAsync();
                Assert.Equal(1, characterCount); // Should still be 1, not reseeded

                Assert.True(await context.PlayerCharacters.AnyAsync(), "PlayerCharacters should be seeded.");
                Assert.True(await context.StoryNodes.AnyAsync(), "StoryNodes should be seeded.");
                Assert.True(await context.Choices.AnyAsync(), "Choices should be seeded.");
                Assert.True(await context.Dialogues.AnyAsync(), "Dialogues should be seeded.");
            }
        }
    }
        
}