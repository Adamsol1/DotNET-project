using System.ComponentModel.DataAnnotations;

namespace backend.Application.Dtos;

public sealed class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    // if we want player and admin role add it here Adam
}

public sealed class RegisterUserDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    // TODO : Should maybe implement a email that is required?
}

public sealed class LoginUserDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}



