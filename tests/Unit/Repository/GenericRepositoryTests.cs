using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using backend.Infrastructure.Data;
using backend.Infrastructure.Repositories;
using backend.Domain.Models;
using Xunit;

namespace tests.Unit.Repository;

public class GenericRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GenericRepository<User> _repository;

    // set up the db connection
    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new GenericRepository<User>(_context);
    }

    [Fact]
    public async Task GetById_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Id = 1, Username = "SpaceMan123", Password = "password123" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("SpaceMan123", result.Username);
        Assert.Equal("password123", result.Password);
    }

    [Fact]
    public async Task GetById_ShouldThrowKeyNotFoundException()
    {
        // Test to see if the user exists
        // if not return a key not found exception set the id to a high unlikely number
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.GetById(99999));
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllUsers()
    {
        // create the objects
        var users = new List<User>
        {
            new User { Id = 1, Username = "Litagoo", Password = "pass1" },
            new User { Id = 2, Username = "Bees", Password = "pass2" }
        };
        await _context.User.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // get form the repository
        var result = await _repository.GetAll();

        // check if the objects match what we expect.
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Create_ShouldAddUserToDatabase()
    {
        // create the user object
        var user = new User { Username = "teees", Password = "passwordpassword" };

        // save to the database
        var result = await _repository.Create(user);

        // check if the user is created
        Assert.NotNull(result);
        Assert.Equal("teees", result.Username);
        Assert.True(result.Id > 0); // EF should assign an ID

        // check if the user is saved to the database
        var savedUser = await _context.User.FindAsync(result.Id);
        Assert.NotNull(savedUser);
        Assert.Equal("teees", savedUser.Username);
    }

    [Fact]
    public async Task Update_ShouldUpdateUser()
    {
        // create the user object
        var user = new User { Id = 1, Username = "StartName", Password = "Password" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        user.Username = "UpdatedName";
        user.Password = "SomePassword";

        // save to the db through the repository
        var result = await _repository.Update(user);

        // check if the values are updated
        Assert.Equal("UpdatedName", result.Username);
        Assert.Equal("SomePassword", result.Password);

        // check if the user is updated in the database
        var updatedUser = await _context.User.FindAsync(1);
        Assert.Equal("UpdatedName", updatedUser.Username);
    }

    [Fact]
    public async Task Delete_ShouldDeleteUser()
    {
        // create the user object
        var user = new User { Id = 1, Username = "DeleteMe", Password = "Passworrrrd" };
        
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // delete the created objet from the database
        var result = await _repository.Delete(1);

        // check if it is what we delete
        Assert.Equal("DeleteMe", result.Username);

        // Verify it was actually deleted from database
        var deletedUser = await _context.User.FindAsync(1);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task Delete_ShouldThrowException()
    {
        // delete a user that does not exist should trow an error.
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _repository.Delete(99999));
    }

    [Fact]
    public async Task GetByProperty_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Id = 1, Username = "UserUser", Password = "password" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProperty(u => u.Username, "UserUser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UserUser", result.Username);
    }

    [Fact]
    public async Task GetByProperty_ShouldReturnNull_WhenPropertyDoesNotMatch()
    {
        // Arrange
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByProperty(u => u.Username, "nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllByProperty_ShouldReturnUsers()
    {
        // create the object
        var users = new List<User>
        {
            new User { Id = 1, Username = "admin", Password = "passmann", Role = UserRole.admin },
            new User { Id = 2, Username = "user", Password = "passwoman", Role = UserRole.player },
            new User { Id = 3, Username = "ryan", Password = "passpass", Role = UserRole.admin }
        };
        await _context.User.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // get the user objects where the user is an admin
        var result = await _repository.GetAllByProperty(u => u.Role, UserRole.admin);

        // should return two objects as there is two admins.
        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(UserRole.admin, u.Role));
    }

    [Fact]
    public async Task GetPropertyValue_WhenUserExists()
    {
        // create the user object
        var user = new User { Id = 1, Username = "MeeMann", Password = "shhsoqso" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the object value from the repository
        var username = await _repository.GetPropertyValue(1, u => u.Username);
        var password = await _repository.GetPropertyValue(1, u => u.Password);

        // check if the value is what we expect
        Assert.Equal("MeeMann", username);
        Assert.Equal("shhsoqso", password);
    }

    [Fact]
    public async Task GetPropertyValue_ShouldReturnNull()
    {
        // get the object from the repository
        var result = await _repository.GetPropertyValue(99999, u => u.Username);

        // check if the value object is null.
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
