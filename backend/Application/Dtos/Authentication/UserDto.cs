using System.ComponentModel.DataAnnotations;

namespace backend.Application.Dtos.Authentication;

public class UserDto
{
    public int Id { get; set; }
 
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]   
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    [CustomValidation(typeof(UserDto), nameof(ValidateUsernameStart))]
    public string Username { get; set; } = string.Empty;
    
    // Custom validation method for username starting with underscore
    public static ValidationResult? ValidateUsernameStart(string username, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ValidationResult.Success; // Required attribute handles this
        }
        
        if (username.StartsWith("_"))
        {
            return new ValidationResult("Username cannot start with an underscore.");
        }
        
        return ValidationResult.Success;
    }
}

public sealed class RegisterUserDto
{
    private string _username = string.Empty;
    
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    [CustomValidation(typeof(RegisterUserDto), nameof(ValidateUsernameStart))]
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
 
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?\"":{{}}|<>])", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;
    
    // Custom validation method for username starting with underscore
    public static ValidationResult? ValidateUsernameStart(string username, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ValidationResult.Success;
        }
        
        if (username.StartsWith("_"))
        {
            return new ValidationResult("Username cannot start with an underscore.");
        }
        
        return ValidationResult.Success;
    }
}

public sealed class LoginUserDto
{
    private string _username = string.Empty;
    
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    [CustomValidation(typeof(LoginUserDto), nameof(ValidateUsernameStart))]
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
 
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?\"":{{}}|<>])", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;
    
    // Custom validation method for username starting with underscore
    public static ValidationResult? ValidateUsernameStart(string username, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ValidationResult.Success;
        }
        
        if (username.StartsWith("_"))
        {
            return new ValidationResult("Username cannot start with an underscore.");
        }
        
        return ValidationResult.Success;
    }
}

public sealed class UpdatePasswordDto
{
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?\"":{{}}|<>])", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (!@#$%^&*(),.?\":{}|<>)")]
    public string NewPassword { get; set; } = string.Empty;
 
    [Required(ErrorMessage = "Confirm password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*(),.?\"":{{}}|<>])", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (!@#$%^&*(),.?\":{}|<>)")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class UpdateUsernameDto
{
    private string _username = string.Empty;
    
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    [CustomValidation(typeof(UpdateUsernameDto), nameof(ValidateUsernameStart))]
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
    
    // Custom validation method for username starting with underscore
    public static ValidationResult? ValidateUsernameStart(string username, ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return ValidationResult.Success;
        }
        
        if (username.StartsWith("_"))
        {
            return new ValidationResult("Username cannot start with an underscore.");
        }
        
        return ValidationResult.Success;
    }
}

