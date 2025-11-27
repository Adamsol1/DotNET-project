using Microsoft.EntityFrameworkCore;
using backend.Domain.Models;

namespace backend.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Character> Characters { get; set; } = null!;
    public DbSet<StoryNode> StoryNodes { get; set; } = null!;
    public DbSet<Dialogue> Dialogues { get; set; } = null!;
    public DbSet<Choice> Choices { get; set; } = null!;
    public DbSet<PlayerCharacter> PlayerCharacters { get; set; } = null!;
    public DbSet<User> User { get; set; } = null!;
    
    public DbSet<GameSave> GameSaves { get; set; }
    

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
        
        modelBuilder.Entity<User>()
            .Property(u => u.AuthUserId)
            .HasMaxLength(450)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.AuthUserId)
            .IsUnique();
        
        
        // This will have to be fixed later on when we get the save entity :)
        //
        // modelBuilder.Entity<PlayerCharacter>()
        //     .HasOne(pc => pc.User)
        //     .WithOne(u => u.PlayerCharacter)
        //     .HasForeignKey<User>(u => u.Id)
        //     .OnDelete(DeleteBehavior.Cascade);
        //
        // modelBuilder.Entity<User>()
        //     .HasMany(u => u.PlayerCharacter)
        //     .WithOne(pc => pc.User)
        //     .HasForeignKey<PlayerCharacter>(pc => pc.UserId)
        //
        //
        
        
        //TODO: Uncomment these when GameSave (save progresstions) are to be implemented
    //     
    //     //Gamesave rel with user
    //     modelBuilder.Entity<GameSave>()
    //         .HasOne(gs => gs.User)
    //         .WithMany(u => u.GameSaves)
    //         .HasForeignKey(gs => gs.UserId)
    //         .OnDelete(DeleteBehavior.Cascade);
    //     
    //     //Gamesave rel with player character
    //     modelBuilder.Entity<GameSave>()
    //         .HasOne(gs => gs.PlayerCharacter)
    //         .WithOne()
    //         .HasForeignKey<GameSave>(gs => gs.PlayerCharacterId)
    //         .OnDelete(DeleteBehavior.Cascade);
    //     
    //     //Gamesave rel with story node
    //     modelBuilder.Entity<GameSave>()
    //         .HasOne(gs => gs.CurrentStoryNode)
    //         .WithMany()
    //         .HasForeignKey(gs => gs.CurrentStoryNodeId)
    //         .OnDelete(DeleteBehavior.Restrict);
    
    }
}