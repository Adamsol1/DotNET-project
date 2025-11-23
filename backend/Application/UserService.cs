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

    public async Task<UserDto> RegisterAccount(RegisterUserDto registerUserDto)
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
    
    public async Task<UserDto> Login(LoginUserDto loginUserDto)
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
}

    