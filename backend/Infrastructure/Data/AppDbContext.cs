using Microsoft.EntityFrameworkCore;
using backend.Domain.Models;

namespace backend.Infrastructure.Data;


/// <summary>
/// Database context for the application setting up the entities and their relationships.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    // DbSets for each entity
    public DbSet<Character> Characters { get; set; } = null!;
    public DbSet<StoryNode> StoryNodes { get; set; } = null!;
    public DbSet<Dialogue> Dialogues { get; set; } = null!;
    public DbSet<Choice> Choices { get; set; } = null!;
    public DbSet<PlayerCharacter> PlayerCharacters { get; set; } = null!;
    public DbSet<User> User { get; set; } = null!;
    public DbSet<GameSave> GameSaves { get; set; }
    
    // Configuring entity relationships and constraints
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Choice rel with StoryNode
        modelBuilder.Entity<Choice>()
            .HasOne(c => c.StoryNode)
            .WithMany(sn => sn.Choices)
            .HasForeignKey(c => c.StoryNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Choice rel with NextStoryNode
        modelBuilder.Entity<Choice>()
            .HasOne(c => c.NextStoryNode)
            .WithMany()
            .HasForeignKey(c => c.NextStoryNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Dialogue rel with Character
        modelBuilder.Entity<Dialogue>()
            .HasOne(d => d.Character)
            .WithMany(c => c.Dialogues)
            .HasForeignKey(d => d.CharacterId)
            .OnDelete(DeleteBehavior.SetNull);

        // Dialogue rel with StoryNode
        modelBuilder.Entity<Dialogue>()
            .HasOne(d => d.StoryNode)
            .WithMany(sn => sn.Dialogues)
            .HasForeignKey(d => d.StoryNodeId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        // PlayerCharacter rel with Character
        modelBuilder.Entity<PlayerCharacter>()
            .HasBaseType<Character>();
        
        // User AuthUserId constraints
        modelBuilder.Entity<User>()
            .Property(u => u.AuthUserId)
            .HasMaxLength(450)
            .IsRequired();
            
        // Unique index on AuthUserId
        modelBuilder.Entity<User>()
            .HasIndex(u => u.AuthUserId)
            .IsUnique();
    
    }
}