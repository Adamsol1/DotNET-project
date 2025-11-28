using backend.Application.Dtos;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Domain.Models;
using backend.Infrastructure.Repositories;
using Serilog;

namespace backend.Application;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork uow, ILogger<UserService> logger)
    {
        _logger = logger;
        _uow = uow; 
    }

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

    private static UserDto ReturnUserDto(User user) => new UserDto
        {
            Id = user.Id,
            Username = user.Username
        };

    public async Task<UserDto> GetUserById(int id)
    {
        try
        {
            var user = await _uow.UserRepository.GetById(id);

            if (user == null)
            {
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
    public async Task<UserDto> UpdateUsername(string authUserId, UpdateUsernameDto updateUsernameDto)
    {
        try
        {
            _logger.LogInformation("[Userservice] UpdateUsername called for AuthUserId: {AuthUserId}", authUserId);
            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByProperty(u => u.AuthUserId, authUserId);
            
            _logger.LogInformation("[Userservice] User lookup result: {UserFound}", user != null ? $"Found user ID {user.Id}" : "Not found");
            
            if (user == null)
            {
                _logger.LogWarning("[Userservice] User with AuthUserId {AuthUserId} not found", authUserId);
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found. Please log out and log back in, or re-register your account.");
            }

            // Check if the new username already exists
            var existingUser = await _uow.UserRepository.GetUserByUsername(updateUsernameDto.Username);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                _logger.LogWarning("[Userservice] Username {Username} already exists", updateUsernameDto.Username);
                await _uow.RollBackAsync();
                throw new InvalidOperationException($"Username already exists.");
            }

            // Update the username
            user.Username = updateUsernameDto.Username;
            await _uow.UserRepository.Update(user);
            
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return ReturnUserDto(user);
        }
        catch (Exception e)
        {
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error updating username for AuthUserId {AuthUserId}", authUserId);
            throw;
        }
    }

    //TODO  FOR ALL CRUD : maybe implement one transaction for both auth user and game user. 
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

            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByProperty(u => u.AuthUserId, authUserId);
            
            if (user == null)
            {
                _logger.LogWarning("[Userservice] User with AuthUserId {AuthUserId} not found", authUserId);
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found. Please log out and log back in, or re-register your account.");
            }

            // Update the password (Note: In production, this should be hashed)
            user.Password = updatePasswordDto.NewPassword;
            await _uow.UserRepository.Update(user);
            
            await _uow.SaveAsync();
            await _uow.CommitAsync();

            return true;
        }
        catch (Exception e)
        {
            await _uow.RollBackAsync();
            _logger.LogError(e, "[Userservice] Error updating password for AuthUserId {AuthUserId}", authUserId);
            throw;
        }
    }

    public async Task<bool> DeleteAccount(string authUserId)
    {
        try
        {
            _logger.LogInformation("[Userservice] DeleteAccount called for AuthUserId: {AuthUserId}", authUserId);
            await _uow.BeginAsync();

            // Get the user by AuthUserId
            var user = await _uow.UserRepository.GetByProperty(u => u.AuthUserId, authUserId);
            
            if (user == null)
            {
                _logger.LogWarning("[Userservice] User with AuthUserId {AuthUserId} not found", authUserId);
                await _uow.RollBackAsync();
                throw new KeyNotFoundException($"User not found.");
            }

            _logger.LogInformation("[Userservice] Deleting user: Id={UserId}, Username={Username}", user.Id, user.Username);
            
            // Delete the user from the game database
            await _uow.UserRepository.Delete(user.Id);
            
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
}
    