using backend.Application.Dtos.Authentication;

namespace backend.Application.Interfaces;
    
    /// <summary>
    /// Service for handling user account and authentication operations.
    /// This includes registration, login, password management, and account updates.
    /// </summary>

public interface IUserService
{

    Task<UserDto> RegisterAccount(RegisterUserDto registerUserDto);
    Task<UserDto?> Login(LoginUserDto loginUserDto);

    // Account management methods

    //Update the username
    Task<UserDto> UpdateUsername(string authUserId, UpdateUsernameDto updateUsernameDto);
    //Update the password
    Task<bool> UpdatePassword(string authUserId, UpdatePasswordDto updatePasswordDto);
    //Delete the account
    Task<bool> DeleteAccount(string authUserId);
    
    // Admin methods

    //Get all users
    Task<IEnumerable<UserDto>> GetAllUsers();
    // get by userId, usefull when clicking on an user.
    Task<UserDto> GetUserById(int id);

    // get auth user id by user id
    Task<string> GetByAuthId(int userId);
    
}