using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Domain.Models;

namespace tests;

public class DatabaseTestHelper
{
    public static async Task<bool> TestGameSaveTracking()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=test.db")
            .Options;

        using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        // Test data
        var gameSave = new GameSave
        {
            UserId = 1,
            PlayerCharacterId = 1,
            CurrentStoryNodeId = 1,
            VisitedNodeIds = "[]",
            LastChoiceId = null,
            CurrentDialogueIndex = 0,
            SaveName = "Test Save",
            LastUpdate = DateTime.UtcNow
        };

        context.GameSaves.Add(gameSave);
        await context.SaveChangesAsync();

        // Verify data was saved
        var savedGameSave = await context.GameSaves.FindAsync(gameSave.Id);
        
        return savedGameSave != null && 
               savedGameSave.VisitedNodeIds == "[]" &&
               savedGameSave.LastChoiceId == null &&
               savedGameSave.CurrentDialogueIndex == 0;
    }
}
