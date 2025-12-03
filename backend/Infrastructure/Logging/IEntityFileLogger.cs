using System.Text.Json;

namespace backend.Infrastructure.Logging;

/// <summary>
/// Logging structure for entity actions, such as creation, update, deletion.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EntityLog<T>
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EntityType { get; set; } = typeof(T).Name;
    public string Action { get; set; } = default!;
    public T Data { get; set; } = default!;
}

/// <summary>
/// Logger interface for logging entity actions to files.
/// </summary>
public interface IEntityFileLogger
{
    Task LogAsync<T>(string action, T entity, string? category = null, string? logName = null);
}

/// <summary>
/// File-based logger implementation for entity actions.
/// </summary>
public class EntityFileLogger : IEntityFileLogger
{
    private readonly string _basePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    //Constructor
    public EntityFileLogger(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(
            env.ContentRootPath, 
            "Logs", 
            "Entities"
        );
        Directory.CreateDirectory(_basePath);
    }

    

    // Logs an entity action to a file.
    public async Task LogAsync<T>(
        string action, 
        T entity, 
        string? category = null,
        string? logName = null
    )
    {
        // Sanitize log name if provided
        try
        {
            var logEntry = new EntityLog<T>
        {
            Action = action,
            Data = entity
        };

        // Determine file path
        var folderPath = _basePath;
        
        // If logName is provided, create a subdirectory
        Directory.CreateDirectory(folderPath);

        // Sanitize category for file naming
        var fileName = string.IsNullOrEmpty(category) 
            ? $"Default-{DateTime.UtcNow:yyyyMMdd}.log"
            : $"{category}-{DateTime.UtcNow:yyyyMMdd}.log";
        
        
        var filePath = Path.Combine(folderPath, fileName);

            var json = JsonSerializer.Serialize(
                logEntry,
                new JsonSerializerOptions { WriteIndented = false }
            ) + Environment.NewLine;

            await _semaphore.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(filePath, json);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[EntityFileLogger] Failed to log: {ex.Message}"
            );
        }
    }
    
}