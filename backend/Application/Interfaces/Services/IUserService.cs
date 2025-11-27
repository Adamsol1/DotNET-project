using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Application.Dtos;

namespace backend.Application.Interfaces.Services;

public interface IUserService
{

    /*
    These are the methods I belive we would need for the UserService.
    Adam can add more if he wants. or tell me what he needs for auth, like session management etc.

    RegisterAccount() -> registers a new user asynchronously from input.
    Login() -> logs in a user and creates a session.
    Logout() -> logs out a user and destroys the session.
    ChangePassword() -> changes a user's password.

    // admin methods ?
    GetAllUsers() -> gets all users.
    GetUserById() -> gets a user by id. 
    checkUserRole() -> checks a user's role.
    UpdateUser() -> updates a user.
    DeleteUser() -> deletes a user.
    */

    Task<UserDto> RegisterAccount(RegisterUserDto registerUserDto, string AuthUserId);
    Task<UserDto?> Login(LoginUserDto loginUserDto);
    //Task<bool> Logout(int userId);
    //Task<bool> ChangePassword(int userId, string oldPassword, string newPassword);
    
    // Admin methods
    //Task<IEnumerable<UserDto>> GetAllUsers();
    Task<UserDto> GetUserById(int id);
    
    //Task<bool> CheckUserRole(int userId, string role);
    //Task<UserDto> UpdateUser(int id, string username);
    //Task<bool> DeleteUser(int id);
}