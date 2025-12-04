using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories.Implementations;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using Moq;
using Xunit;

namespace tests.Unit.Repository;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;
    private readonly Mock<IEntityFileLogger> _mockLogger;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _mockLogger = new Mock<IEntityFileLogger>();
        _repository = new UserRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnUser()
    {
        // create and save the user object
        var user = new User { Id = 1, Username = "testuser", AuthUserId = "auth-user-1" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the user object from the repository
        var result = await _repository.GetUserByUsername("testuser");

        // check if the result is what we expect
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("auth-user-1", result.AuthUserId);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnNull()
    {
        // create the user object
        var user = new User { Id = 1, Username = "testuser", AuthUserId = "auth-user-1" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the user object from the repository
        var result = await _repository.GetUserByUsername("doesnotexist");

        // check if the result is null
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUsernameById_ShouldReturnUsername()
    {
        // create the user object
        var user = new User { Id = 1, Username = "testuser", AuthUserId = "auth-user-1" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the username from the repository
        var result = await _repository.GetUsernameById(1);

        // check if the result is what we expect
        Assert.Equal("testuser", result);
    }

    [Fact]
    public async Task GetUsernameById_ShouldReturnNull()
    {
        // get the username from the repository
        var result = await _repository.GetUsernameById(999);

        // check if the result is null
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldUseGenericMethod()
    {
        // create the object
        var user = new User { Id = 1, Username = "testuser", AuthUserId = "auth-user-1" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the user from the db repo
        var result = await _repository.GetUserByUsername("testuser");

        // expect if the value is what we want
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUsernameById_ShouldUseGenericMethod()
    {
        // create the object
        var user = new User { Id = 1, Username = "testuser", AuthUserId = "auth-user-1" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the username from the db repo
        var result = await _repository.GetUsernameById(1);

        // check if the result matches the username of the user
        Assert.Equal("testuser", result);
    }

    [Fact]
    public async Task Create_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var user = new User 
        { 
            Username = "newuser", 
            AuthUserId = "auth-newuser-1",
            Role = UserRole.player
        };

        // Act
        var result = await _repository.Create(user);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("newuser", result.Username);
        Assert.Equal("auth-newuser-1", result.AuthUserId);
        Assert.Equal(UserRole.player, result.Role);

        // Verify it was saved to database
        var savedUser = await _context.User.FindAsync(result.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("newuser", savedUser!.Username);
    }

    [Fact]
    public async Task Create_ShouldCreateUser_EvenWithEmptyUsername()
    {
        // Arrange - InMemory database doesn't enforce constraints
        // This test verifies the repository doesn't throw, but in production
        // validation should happen at the service/DTO level
        var user = new User 
        { 
            Username = string.Empty,
            AuthUserId = "auth-empty-1"
        };

        // Act
        var result = await _repository.Create(user);

        // Assert - InMemory allows empty strings, but production should validate
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(string.Empty, result.Username);
    }

    [Fact]
    public async Task Update_ShouldUpdateUserSuccessfully()
    {
        // Arrange
        var user = new User 
        { 
            Id = 1,
            Username = "originaluser", 
            AuthUserId = "auth-original-1",
            Role = UserRole.player
        };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        user.Username = "updateduser";
        user.AuthUserId = "auth-updated-1";
        user.Role = UserRole.admin;
        var result = await _repository.Update(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("updateduser", result.Username);
        Assert.Equal("auth-updated-1", result.AuthUserId);
        Assert.Equal(UserRole.admin, result.Role);

        // Verify it was updated in database
        var updatedUser = await _context.User.FindAsync(1);
        Assert.NotNull(updatedUser);
        Assert.Equal("updateduser", updatedUser!.Username);
        Assert.Equal("auth-updated-1", updatedUser.AuthUserId);
        Assert.Equal(UserRole.admin, updatedUser.Role);
    }

    [Fact]
    public async Task Update_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
    {
        // Arrange
        var user = new User 
        { 
            Id = 99999,
            Username = "nonexistent", 
            AuthUserId = "auth-nonexistent-1"
        };

        // Act & Assert
        // Update method throws InvalidOperationException when trying to update non-existent entity
        // (wraps the underlying DbUpdateException from the database)
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.Update(user));
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnUser_ForLogin()
    {
        // Arrange - simulate login scenario
        var user = new User 
        { 
            Id = 1,
            Username = "loginuser", 
            AuthUserId = "auth-login-1",
            Role = UserRole.player
        };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act - attempt to find user by username (login lookup)
        var result = await _repository.GetUserByUsername("loginuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("loginuser", result.Username);
        Assert.Equal("auth-login-1", result.AuthUserId);
        Assert.Equal(UserRole.player, result.Role);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnNull_WhenUserNotFound_ForLogin()
    {
        // Arrange - no user in database

        // Act - attempt to find non-existent user (failed login)
        var result = await _repository.GetUserByUsername("nonexistentuser");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByAuthId_ShouldReturnUser_ForLogin()
    {
        // Arrange - simulate login scenario using AuthUserId
        var user = new User 
        { 
            Id = 1,
            Username = "authuser", 
            AuthUserId = "identity-user-123",
            Role = UserRole.player
        };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act - attempt to find user by AuthUserId (login lookup)
        var result = await _repository.GetByAuthId("identity-user-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("authuser", result.Username);
        Assert.Equal("identity-user-123", result.AuthUserId);
    }

    [Fact]
    public async Task GetByAuthId_ShouldReturnNull_WhenUserNotFound_ForLogin()
    {
        // Arrange - no user in database

        // Act - attempt to find non-existent user by AuthUserId (failed login)
        var result = await _repository.GetByAuthId("nonexistent-auth-id");

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
