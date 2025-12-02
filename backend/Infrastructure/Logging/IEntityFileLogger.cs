using System.Text.Json;

namespace backend.Infrastructure.Logging;

// Add this class first
public class EntityLog<T>
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EntityType { get; set; } = typeof(T).Name;
    public string Action { get; set; } = default!;
    public T Data { get; set; } = default!;
}

public interface IEntityFileLogger
{
    Task LogAsync<T>(string action, T entity, string? category = null, string? logName = null);
}

public class EntityFileLogger : IEntityFileLogger
{
    private readonly string _basePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public EntityFileLogger(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(
            env.ContentRootPath, 
            "Logs", 
            "Entities"
        );
        Directory.CreateDirectory(_basePath);
    }

    

    public async Task LogAsync<T>(
        string action, 
        T entity, 
        string? category = null,
        string? logName = null
    )
    {
        try
        {
            var logEntry = new EntityLog<T>
        {
            Action = action,
            Data = entity
        };

        var folderPath = string.IsNullOrEmpty(category) 
            ? _basePath 
            : Path.Combine(_basePath, category);
        
        Directory.CreateDirectory(folderPath);

        var typeName = logName ?? SanitizeFileName(typeof(T).Name);
        var fileName = $"{typeName}-{DateTime.UtcNow:yyyyMMdd}.log";
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
    private static string SanitizeFileName(string fileName)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    return string.Concat(
        fileName.Select(c => invalidChars.Contains(c) ? '_' : c)
    );
}
}