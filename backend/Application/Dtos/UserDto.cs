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
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginUserDto
{
    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    public string Password { get; set; } = string.Empty;
}

