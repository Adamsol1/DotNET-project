using System.ComponentModel.DataAnnotations;


namespace backend.ViewModels;

public class LogInViewModel
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}