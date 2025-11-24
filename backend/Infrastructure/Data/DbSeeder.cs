using System.Text.Json;
using backend.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data;

public static class DbSeeder
{
   /// <summary>
   /// Seed the database with initial data from JSON files if the tables are empty. 
   /// The method also applies any pending migrations to the database.
   /// The JSON files should be located in the "wwwroot/seedData" directory.
   /// </summary>
   /// <param name="context"></param>
   /// <returns></returns>
    public static async Task SeedAsync(AppDbContext context)
    {
        // Deletes database before seeding
        //await context.Database.EnsureDeletedAsync();
        
        // Apply migrations
        await context.Database.MigrateAsync();
        
        
        //1. user
        if (!await context.User.AnyAsync())
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "seedData", "Users.json");
            var jsonData = await File.ReadAllTextAsync(path);
            var users = JsonSerializer.Deserialize<List<User>>(jsonData);

            if (users != null && users.Count > 0)
            {
                await context.User.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }
        
        // 2. StoryNodes
        if (!await context.StoryNodes.AnyAsync())
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "seedData", "StoryNode.json");
            var jsonData = await File.ReadAllTextAsync(path);
            var storyNodes = JsonSerializer.Deserialize<List<StoryNode>>(jsonData);

            if (storyNodes != null && storyNodes.Count > 0)
            {
                await context.StoryNodes.AddRangeAsync(storyNodes);
                await context.SaveChangesAsync();
            }
        }
        
        
        // 3. Characters
        if (!await context.Characters.AnyAsync())
        {
            // Path to the JSON file
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "seedData", "Character.json");
            var jsonData = await File.ReadAllTextAsync(path);
            var characters = JsonSerializer.Deserialize<List<Character>>(jsonData);

            // If there are characters in the JSON file, add them to the database
            if (characters != null && characters.Count > 0)
            {
                await context.Characters.AddRangeAsync(characters);
                await context.SaveChangesAsync();
            }
        }


        // 4. PlayerCharacters
        if (!await context.PlayerCharacters.AnyAsync())
        {
            var path = Path.Combine(AppContext.BaseDirectory,  "wwwroot", "seedData", "PlayerCharacter.json");
            var jsonData = await File.ReadAllTextAsync(path);
            var playerCharacters = JsonSerializer.Deserialize<List<PlayerCharacter>>(jsonData);

            if (playerCharacters != null && playerCharacters.Count > 0)
            {
                await context.PlayerCharacters.AddRangeAsync(playerCharacters);
                await context.SaveChangesAsync();
            }
        }


        


        // 5. Choices
        if (!await context.Choices.AnyAsync())
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "seedData", "Choice.json");
            var jsonData = await File.ReadAllTextAsync(path);
            var choices = JsonSerializer.Deserialize<List<Choice>>(jsonData);

            if (choices != null && choices.Count > 0)
            {
                await context.Choices.AddRangeAsync(choices);
                await context.SaveChangesAsync();
            }
        }

        // 6. Dialogues (depends on Characters & StoryNodes)
        if (!await context.Dialogues.AnyAsync())
        {
            var path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "seedData", "Dialogue.json");
            var jsonData = await File.ReadAllTextAsync(path);
            var dialogues = JsonSerializer.Deserialize<List<Dialogue>>(jsonData);

            if (dialogues != null && dialogues.Count > 0)
            {
                await context.Dialogues.AddRangeAsync(dialogues);
                await context.SaveChangesAsync();
            }
        }
    }
}
