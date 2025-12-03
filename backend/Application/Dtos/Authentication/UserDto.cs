using System.ComponentModel.DataAnnotations;

namespace backend.Application.Dtos.Authentication;

/// <summary>
/// User Data Transfer Object that represents a user in the system. With validation attributes. 
/// </summary>
public class UserDto
{
    public int Id { get; set; }
 
    // Requiredments for username:
    // - Must be between 3 and 20 characters long.
    // - Can only contain letters, numbers, and underscores.
    // - Cannot start with an underscore.
    // - Case insensitive (stored as lowercase).
    // - Required field.equired(ErrorMessage = "Username is required.")
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]   
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    [CustomValidation(typeof(UserDto), nameof(ValidateUsernameStart))]
    public string Username { get; set; } = string.Empty;
    
      
    // Custom validation method for username starting with underscoreblic static ValidationResult? ValidateUsernameStart(string username, ValidationContext context)
    public static ValidationResult? ValidateUsernameStart(string username, ValidationContext context){
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
/// DTO for registering a new user
public sealed class RegisterUserDto
{
    private string _username = string.Empty;
    
    // Requiredments for username:
    // - Must be between 3 and 20 characters long.
    // - Can only contain letters, numbers, and underscores.
    // - Cannot start with an underscore.
    // - Case insensitive (stored as lowercase).
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    [CustomValidation(typeof(RegisterUserDto), nameof(ValidateUsernameStart))]
    public string Username
    {
        get => _username;
        set => _username = value?.ToLowerInvariant() ?? string.Empty;
    }
 
    // Requiredments for password:
    // - Must be between 8 and 50 characters long.
    // - Must contain at least one uppercase letter, one lowercase letter, one number, and one special character.
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=.,:?])[A-Za-z0-9!@#$%^&*()_\-+=.,:?]{8,50}$", 
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

/// DTO for logging in a user
public sealed class LoginUserDto
{
    private string _username = string.Empty;

    // Requiredments for username:
    // - Must be between 3 and 20 characters long.
    // - Can only contain letters, numbers, and underscores.
    // - Cannot start with an underscore.
    // - Case insensitive (stored as lowercase).    
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
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=.,:?])[A-Za-z0-9!@#$%^&*()_\-+=.,:?]{8,50}$", 
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

/// DTO for updating user password
public sealed class UpdatePasswordDto
{
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=.,:?])[A-Za-z0-9!@#$%^&*()_\-+=.,:?]{8,50}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string NewPassword { get; set; } = string.Empty;
 
    [Required(ErrorMessage = "Confirm password is required.")]
    [StringLength(50, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 50 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=.,:?])[A-Za-z0-9!@#$%^&*()_\-+=.,:?]{8,50}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// DTO for updating username
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

