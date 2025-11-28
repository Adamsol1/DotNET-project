using System.ComponentModel.DataAnnotations;

namespace backend.Application.Dtos;

public sealed class UserDto
{
    public int Id { get; set; }
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    // if we want player and admin role add it here Adam
}

public sealed class RegisterUserDto
{
    [Required]
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    public string Password { get; set; } = string.Empty;
    // TODO : Should maybe implement a email that is required?
}

public sealed class LoginUserDto
{
    [Required]
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    public string Password { get; set; } = string.Empty;
}

public sealed class UpdateUsernameDto
{
    [Required]
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
}

public sealed class UpdatePasswordDto
{
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}


