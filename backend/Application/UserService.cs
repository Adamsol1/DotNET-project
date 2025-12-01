using backend.Application.Dtos;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Domain.Models;
using backend.Infrastructure.Repositories;
using Serilog;
using Microsoft.AspNetCore.Identity;

namespace backend.Application;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<AuthUser> _userManager;


    public UserService(IUnitOfWork uow, ILogger<UserService> logger, UserManager<AuthUser> userManager)
    {
        _logger = logger;
        _uow = uow;
        _userManager = userManager;

    }

    /*
    Decided to split this file in two parts.

    1. Account management methods. used by users / players to manage themself.

    2. admin methods used in admin controller. used by an admin to manage accounts.
    these methods are also little more different then the account management methods.

    (have yeet to make changes on methods made by Eirik)

    */

    // register account - User
    public async Task<UserDto> RegisterAccount(RegisterUserDto registerUserDto, string AuthUserId)
    {
        try{

            await _uow.BeginAsync();

            //check if the username already exists if it does, rollback the transaction.
            // and trow error.
            var existingUser = await _uow.UserRepository.GetUserByUsername(registerUserDto.Username);

            if(existingUser != null)
            {
                _logger.LogWarning("[Userservice] User with username already exists", registerUserDto.Username);
                await _uow.RollBackAsync();
                throw new InvalidOperationException($"User with username already exists.");
            }


            var user = new User
            {
                Username = registerUserDto.Username,
                Password = registerUserDto.Password,
                AuthUserId = AuthUserId

            };

            await _uow.UserRepository.Create(user);

            // save changes & commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return ReturnUserDto(user);
        }
        catch ( Exception e)
        {
            // throw error and rollback transaction which disposes of it
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error registering account");
            throw;
        }
        
    }
    
    // login - User
    public async Task<UserDto?> Login(LoginUserDto loginUserDto)
    {
        try
        {
            var user = await _uow.UserRepository.GetUserByUsername(loginUserDto.Username);
            //Check if user exists
            if(user == null)
            {
                _logger.LogWarning("[Userservice] User with username {Username} doesn't exist", loginUserDto.Username);
                return null;
            }

            //validate the password
            if (user.Password != loginUserDto.Password)
            {
                _logger.LogWarning("[Userservice] Invalid password for user {Username}", loginUserDto.Username);
                return null;
            }

            return ReturnUserDto(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[Userservice] Error during login for username {Username}", loginUserDto.Username);
            throw;
        }
        
    }

    // update username - User and Admin
    /* trying overhaul on this method.  
    the intial method did not update the username in auth db to
    this resulted to that the user transaction in game was rolled back
    Auth would then update the name, causing a mismatch.

    // moved UserManager into here instead of in the controller, as previously done.
    -Ah
    */
    public async Task<UserDto> UpdateUsername( string authUserId, UpdateUsernameDto updateUsernameDto )
    {

        try {

            // start a transaction
            await _uow.BeginAsync();
            _logger.LogInformation("[Userservice] UpdateUsername called for AuthUserId: {authUserId}", authUserId);

            // get the user by auth user id
            var user = await _uow.UserRepository.GetByAuthId(authUserId);

            // log the result
            _logger.LogInformation("[Userservice] User lookup result: {UserFound}", user != null ? $"Found user ID {user.Id}" : "Not found");

            // check if the user exists
            if (user == null) {
                _logger.LogWarning("[Userservice] User not found.");
                
                // if user doesnt exists we dont need the transaction as we dont make changes
                await _uow.RollBackAsync();

                // throw an error
                throw new KeyNotFoundException("User was not found.");
            }

            // check if the new username already exists
            //we cant have to users named the same.
            var exists = await _uow.UserRepository.GetUserByUsername(updateUsernameDto.Username);

            if (exists != null && exists.Id != user.Id) {
                _logger.LogWarning("Username already exists.");
                // throw the transaction back
                await _uow.RollBackAsync();
                // throw an error
                throw new InvalidOperationException("Username already exists.");
            }

            // update the username in the game database aswell as the auth database.
            user.Username = updateUsernameDto.Username;
            await _uow.UserRepository.Update(user);

            // we wait to save the changes to game database before updating the auth database.
            
            // we have the id, check up based on that
            var authUser = await _userManager.FindByIdAsync(user.AuthUserId);
            
            // check if the auth user exists
            if (authUser == null)
            {
                _logger.LogWarning("[Userservice] AuthUser with AuthUserId {AuthUserId} not found in Identity", user.AuthUserId);
                
                // Rollback game database since we can't update AuthUser, as it doesnt exist.
                await _uow.RollBackAsync();

                throw new KeyNotFoundException("AuthUser not found in authentication system. Cannot update username.");
            }

            // update the username in the auth database.
            authUser.UserName = updateUsernameDto.Username;

            var result = await _userManager.UpdateAsync(authUser);

            // check if the result succeeded if not we rollback on gameDB to. 
            if (!result.Succeeded) {
                _logger.LogWarning("Failed to update AuthUser username");

                // rollback the transaction on everything / gamedb as auth database as failed.
                await _uow.RollBackAsync();
                throw new InvalidOperationException("Failed to update username in auth database.");
            }

            // save the changes to database and commit the transaction.
            await _uow.SaveAsync();
            await _uow.CommitAsync();
            
            _logger.LogInformation("[Userservice] Successfully updated username in both databases for AuthUserId {AuthUserId}", authUserId);

            // return a user dto.
            return ReturnUserDto(user);

        } catch (Exception e) 
        {
            // rollback the transaction
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error updating username in both databases", e.Message);
            throw;
        }
    }

    //TODO  FOR ALL CRUD : maybe implement one transaction for both auth user and game user.
    // update password - User & admin
    public async Task<bool> UpdatePassword(string authUserId, UpdatePasswordDto updatePasswordDto)
    {
        try
        {
            // Validate that passwords match
            if (updatePasswordDto.NewPassword != updatePasswordDto.ConfirmPassword)
            {
                _logger.LogWarning("[Userservice] Password confirmation doesn't match for AuthUserId {AuthUserId}", authUserId);
                throw new InvalidOperationException("Passwords do not match.");
            }

            // start a transaction
            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByAuthId(authUserId);

            // check if the user exists
            if (user == null)
            {
                _logger.LogWarning("[Userservice] User with AuthUserId {AuthUserId} not found", authUserId);
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found. Please log out and log back in, or re-register your account.");
            }

            // Update the password (Note: In production, this should be hashed)
            user.Password = updatePasswordDto.NewPassword;
            await _uow.UserRepository.Update(user);

            // wait to save the changes to database before updating the auth database.

            var authUser = await _userManager.FindByIdAsync(user.AuthUserId);

            if (authUser == null)
            {
                _logger.LogWarning("[Userservice] AuthUser with AuthUserId {AuthUserId} not found in Identity", user.AuthUserId);
                await _uow.RollBackAsync();
                throw new KeyNotFoundException("AuthUser not found in authentication system. Cannot update password.");
            }

            // update the password in the auth database.
            var token = await _userManager.GeneratePasswordResetTokenAsync(authUser);
            // reset the password in the auth database.
            var result = await _userManager.ResetPasswordAsync(authUser, token, updatePasswordDto.NewPassword);

            // check if the result succeeded if not we rollback on gameDB to. 

            if (!result.Succeeded) {
                _logger.LogWarning("Failed to update AuthUser password");
                await _uow.RollBackAsync();
                throw new InvalidOperationException("Failed to update password in auth database.");

            }
            
            // save the changes to database and commit the transaction.
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            _logger.LogInformation("[Userservice] Successfully updated password in both databases for AuthUserId {AuthUserId}", authUserId);

            return true;
        }
        catch (Exception e)
        {
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error updating password for AuthUserId {AuthUserId}", authUserId);
            throw;
        }
    }

    // delete account - User & admin
    public async Task<bool> DeleteAccount(string authUserId)
    {
        try
        {
            _logger.LogInformation("[Userservice] DeleteAccount called for AuthUserId: {AuthUserId}", authUserId);
            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByAuthId(authUserId);
            
            if (user == null)
            {
                _logger.LogWarning("[Userservice] User with AuthUserId {AuthUserId} not found", authUserId);
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found.");
            }

            _logger.LogInformation("[Userservice] Deleting user: Id={UserId}, Username={Username}", user.Id, user.Username);
            
            // Delete the user from the game database
            await _uow.UserRepository.Delete(user.Id);

            //moved the AuthUser from UserManager here.
            var authUser = await _userManager.FindByIdAsync(user.AuthUserId);

            // check if user exists, if it doesnt we can just rollback
            // little tricky case, for later - should the user be allowed to delete if
            // there is no auth user, but there is a game user? as I imagine you would need auth user to 
            // have an game account.
            if (authUser == null) {
                
                await _uow.RollBackAsync();
                throw new KeyNotFoundException("AuthUser not found in authentication system. Cannot delete account.");
            }

            var result = await _userManager.DeleteAsync(authUser);

            // check if the result succeeded if not we rollback on gameDB to. 
            if (!result.Succeeded) {
                _logger.LogWarning("Failed to delete AuthUser");
                await _uow.RollBackAsync();
                throw new InvalidOperationException("Failed to delete AuthUser in authentication system. Cannot delete account.");
            }

            // save the changes to database and commit the transaction.
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            _logger.LogInformation("[Userservice] Successfully deleted user with AuthUserId {AuthUserId}", authUserId);
            return true;
        }
        catch (Exception e)
        {
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error deleting account for AuthUserId {AuthUserId}", authUserId);
            throw;
        }
    }

    // return a user dto helper method, move to bottom of the file. 
    private static UserDto ReturnUserDto(User user) => new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };

    // admin methods. 

    // get all users - Admin
    public async Task<IEnumerable<UserDto>> GetAllUsers() 
    {
        try {
            // start a transaction
            await _uow.BeginAsync();

            // get all users by using the generic Repository method all repositories inherit.
            var users = await _uow.UserRepository.GetAll();

            // if no users are found, throw an error.
            if (users == null) {
                _logger.LogWarning("[Userservice] No users found");
                throw new KeyNotFoundException("No users found.");
            }
            
            // return to an dto list.
            return users.Select(ReturnUserDto);
        }
        catch (Exception e)
        {
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error fetching all users");
            throw;
        }
    }

    // get user by id - Admin - user
    public async Task<UserDto> GetUserById(int id)
    {
        try
        {
            var user = await _uow.UserRepository.GetById(id);

            if (user == null)
            {

                // key not found is thrown here so we dont need a catch for it.
                _logger.LogWarning("[Userservice] User with id {id} not found", id);
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            return ReturnUserDto(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[Userservice] Error fetching user with id {id}", id);
            throw;
        }
    }

    public async Task<string> GetByAuthId(int userId)
    {
        try
        {
            var user = await _uow.UserRepository.GetById(userId);
            if (user == null)
            {
                _logger.LogWarning("[Userservice] User with id {userId} not found", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            return user.AuthUserId;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[Userservice] Error fetching AuthUserId for userId {userId}", userId);
            throw;
        }
    }


    // update username - admin
    // making it a little different 

}
    