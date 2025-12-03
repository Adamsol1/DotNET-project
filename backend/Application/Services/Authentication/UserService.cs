using backend.Application.Dtos.Authentication;
using backend.Application.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Identity;

namespace backend.Application.Services.Authentication;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IEntityFileLogger _entityLogger;
    private readonly UserManager<AuthUser> _userManager;


    public UserService(IUnitOfWork uow, IEntityFileLogger entityFileLogger, UserManager<AuthUser> userManager)
    {
        _entityLogger = entityFileLogger;
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
    public async Task<UserDto> RegisterAccount(RegisterUserDto registerUserDto)
    {
        try{

            await _uow.BeginAsync();

            //check if the username already exists if it does, rollback the transaction.
            // and trow error.
            var existingUser = await _uow.UserRepository.GetUserByUsername(registerUserDto.Username);

            if(existingUser != null)
            {
                await _entityLogger.LogAsync(
                    "User with username already exists.",
                    new { Username = registerUserDto.Username, Timestamp = DateTime.UtcNow },
                    LogCategories.Authentication.Registration
                );
                await _uow.RollBackAsync();
                throw new InvalidOperationException($"User with username already exists.");
            }

            // create user in auth database.
            var authUser = new AuthUser {
                UserName = registerUserDto.Username,
            };

            // create the user and password through UserManager.
            var result = await _userManager.CreateAsync(authUser, registerUserDto.Password);

            // check if it has succeeded 
            if (!result.Succeeded) {
                await _entityLogger.LogAsync(
                    "register account error user creation failed",
                    new { Username = registerUserDto.Username, Errors = result.Errors.Select(e => e.Description), Timestamp = DateTime.UtcNow },
                    LogCategories.Authentication.Registration
                );
                // rollback the transaction.
                await _uow.RollBackAsync();
                throw new InvalidOperationException($"Unable to create account with username {registerUserDto.Username}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // get the AuthUserId from the auth user.
            var authUserId = authUser.Id;

            // assign player role to the auth user
            var roleResult = await _userManager.AddToRoleAsync(authUser, "player");
            if (!roleResult.Succeeded)
            {
                await _entityLogger.LogAsync(
                    "register account error role assignment failed",
                    new { Username = registerUserDto.Username, Errors = roleResult.Errors.Select(e => e.Description), Timestamp = DateTime.UtcNow },
                    LogCategories.Authentication.Registration
                );
                await _uow.RollBackAsync();
                await _userManager.DeleteAsync(authUser);
                throw new InvalidOperationException($"Failed to assign role to user. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }

            // create game user with the auth user id
            var user = new User
            {
                Username = registerUserDto.Username,
                AuthUserId = authUserId
            };

            await _uow.UserRepository.Create(user);

            // save changes & commit
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return ReturnUserDto(user);
        }
        catch
        {
            // throw error and rollback transaction which disposes of it
            await _uow.RollBackAsync();
            throw;
        }
        
    }
    
    // login - User
    public async Task<UserDto?> Login(LoginUserDto loginUserDto)
    {
        try
        {
            // First authenticate against AuthDb using UserManager
            var authUser = await _userManager.FindByNameAsync(loginUserDto.Username);
            
            if (authUser == null)
            {
                await _entityLogger.LogAsync(
                    "AuthUser with username does not exist.",
                    new { Username = loginUserDto.Username, Timestamp = DateTime.UtcNow },
                    LogCategories.Authentication.Login
                );
                return null;
            }

            // Validate password against AuthDb
            if (!await _userManager.CheckPasswordAsync(authUser, loginUserDto.Password))
            {
                await _entityLogger.LogAsync(
                    "Invalid password for user.",
                    new { Username = loginUserDto.Username, Timestamp = DateTime.UtcNow },
                    LogCategories.Authentication.Login
                );
                return null;
            }

            // Get the game user by AuthUserId
            var user = await _uow.UserRepository.GetByAuthId(authUser.Id);
            
            //Check if game user exists
            if(user == null)
            {
                await _entityLogger.LogAsync(
                    "GameUser with AuthUserId does not exist.",
                    new { AuthUserId = authUser.Id, Timestamp = DateTime.UtcNow },
                    LogCategories.Authentication.Login
                );
                return null;
            }

            return ReturnUserDto(user);
        }
        catch
        {
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

            // get the user by auth user id
            var user = await _uow.UserRepository.GetByAuthId(authUserId);

            // check if the user exists
            if (user == null) {
                await _entityLogger.LogAsync(
                    "User was not found.",
                    new { AuthUserId = authUserId, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Username
                );
                
                // if user doesnt exists we dont need the transaction as we dont make changes
                await _uow.RollBackAsync();

                // throw an error
                throw new KeyNotFoundException("User was not found.");
            }

            // check if the new username already exists
            //we cant have to users named the same.
            var exists = await _uow.UserRepository.GetUserByUsername(updateUsernameDto.Username);

            if (exists != null && exists.Id != user.Id) {
                await _entityLogger.LogAsync(
                    "Username already exists.",
                    new { Username = updateUsernameDto.Username, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Username
                );
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
                await _entityLogger.LogAsync(
                    "AuthUser not found in authentication system. Cannot update username.",
                    new { AuthUserId = user.AuthUserId, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Username
                );
                
                // Rollback game database since we can't update AuthUser, as it doesnt exist.
                await _uow.RollBackAsync();

                throw new KeyNotFoundException("AuthUser not found in authentication system. Cannot update username.");
            }

            // update the username in the auth database.
            authUser.UserName = updateUsernameDto.Username;

            var result = await _userManager.UpdateAsync(authUser);

            // check if the result succeeded if not we rollback on gameDB to. 
            if (!result.Succeeded) {
                await _entityLogger.LogAsync(
                    $"Failed to update username in auth database.",
                    new { AuthUserId = user.AuthUserId, Errors = result.Errors.Select(e => e.Description), Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Username
                );

                // rollback the transaction on everything / gamedb as auth database as failed.
                await _uow.RollBackAsync();
                throw new InvalidOperationException("Failed to update username in auth database.");
            }

            // save the changes to database and commit the transaction.
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            // return a user dto.
            return ReturnUserDto(user);

        }
        catch
        {
            // rollback the transaction
            await _uow.RollBackAsync();
            throw;
        }
    }

    //TODO  FOR ALL CRUD : maybe implement one transaction for both auth user and game user. 
    // Method to update password for the currently authenticated user
    public async Task<bool> UpdatePassword(string authUserId, UpdatePasswordDto updatePasswordDto)
    {
        try
        {
            // Validate that passwords match
            if (updatePasswordDto.NewPassword != updatePasswordDto.ConfirmPassword)
            {
                await _entityLogger.LogAsync(
                    "Passwords do not match.",
                    new { AuthUserId = authUserId, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Password
                );
                throw new InvalidOperationException("Passwords do not match.");
            }

            // start a transaction
            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByProperty(u => u.AuthUserId, authUserId);
            
            // Check if user exists
            if (user == null)
            {
                await _entityLogger.LogAsync(
                    "User not found. Please log out and log back in, or re-register your account.",
                    new { AuthUserId = authUserId, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Password
                );
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found. Please log out and log back in, or re-register your account.");
            }

            // Password is stored only in AuthDb (Identity), not in game.db
            var authUser = await _userManager.FindByIdAsync(user.AuthUserId);

            if (authUser == null)
            {
                await _entityLogger.LogAsync(
                    "AuthUser not found in authentication system. Cannot update password.",
                    new { AuthUserId = user.AuthUserId, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Password
                );
                await _uow.RollBackAsync();
                throw new KeyNotFoundException("AuthUser not found in authentication system. Cannot update password.");
            }

            // update the password in the auth database.
            var token = await _userManager.GeneratePasswordResetTokenAsync(authUser);
            // reset the password in the auth database.
            var result = await _userManager.ResetPasswordAsync(authUser, token, updatePasswordDto.NewPassword);

            // check if the result succeeded if not we rollback on gameDB to. 

            if (!result.Succeeded) {
                await _entityLogger.LogAsync(
                    "Failed to update password in auth database.",
                    new { AuthUserId = user.AuthUserId, Errors = result.Errors.Select(e => e.Description), Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Password
                );
                await _uow.RollBackAsync();
                throw new InvalidOperationException("Failed to update password in auth database.");

            }
            
            // Password is only stored in AuthDb, so we just commit the transaction
            await _uow.CommitAsync();

            return true;
        }
        catch
        {
            await _uow.RollBackAsync();
            throw;
        }
    }
    // Method to delete the account of the currently authenticated user
    public async Task<bool> DeleteAccount(string authUserId)
    {
        try
        {
            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByAuthId(authUserId);
            
            // Check if user exists
            if (user == null)
            {
                await _entityLogger.LogAsync(
                    "User not found.",
                    new { AuthUserId = authUserId, Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Deletion
                );
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found.");
            }
            
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
                await _entityLogger.LogAsync(
                    "Failed to delete AuthUser in authentication system. Cannot delete account.",
                    new { AuthUserId = user.AuthUserId, Errors = result.Errors.Select(e => e.Description), Timestamp = DateTime.UtcNow },
                    LogCategories.AccountManagement.Deletion
                );
                await _uow.RollBackAsync();
                throw new InvalidOperationException("Failed to delete AuthUser in authentication system. Cannot delete account.");
            }

            // save the changes to database and commit the transaction.
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return true;
        }
        catch
        {
            await _uow.RollBackAsync();
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
                throw new KeyNotFoundException("No users found.");
            }
            
            // return to an dto list.
            return users.Select(ReturnUserDto);
        }
        catch
        {
            await _uow.RollBackAsync();
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
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            return ReturnUserDto(user);
        }
        catch
        {
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
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }
            return user.AuthUserId;
        }
        catch
        {
            throw;
        }
    }


    // update username - admin
    // making it a little different 

}
    