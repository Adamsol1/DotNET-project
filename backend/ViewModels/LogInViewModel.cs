using System.ComponentModel.DataAnnotations;


namespace backend.ViewModels;

public class LogInViewModel
{
    private string _username = string.Empty;

    [Required]
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    [Required]
    public string Password { get; set; }
}
