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

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnUser()
    {
        // create and save the user object
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the user object from the repository
        var result = await _repository.GetUserByUsername("testuser");

        // check if the result is what we expect
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("password123", result.Password);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnNull()
    {
        // create the user object
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
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
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
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
    public async Task GetPasswordById_ShouldReturnPassword()
    {
        // create the user object
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the password from the repository
        var result = await _repository.GetPasswordById(1);

        // check if the result match what we expect 
        Assert.Equal("password123", result);
    }

    [Fact]
    public async Task GetPasswordById_ShouldReturnNull()
    {
        // get the password from the repository
        var result = await _repository.GetPasswordById(99999);

        // check if the result is null
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserRoleById_ShouldReturnRoleString()
    {
        // create the user object
        var user = new User { Id = 1, Username = "admin", Password = "password123", Role = UserRole.admin };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the user role from the repository
        var result = await _repository.GetUserRoleById(99999);  // user does not exist so should return null

        // check if the result is what we expect
        Assert.Equal("admin", result);
    }

    [Fact]
    public async Task GetUserRoleById_ShouldReturnUserRoleString()
    {
        // create the user object
        var user = new User { Id = 1, Username = "user", Password = "password123", Role = UserRole.player };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the user role from the repository
        var result = await _repository.GetUserRoleById(1);

        // check if the result is what we expect
        Assert.Equal("player", result);
    }

    [Fact]
    public async Task GetUserRoleById_ShouldReturnNull()
    {
        // get a high number that does not exist
        var result = await _repository.GetUserRoleById(99999);

        // check if the result is null
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldUseGenericMethod()
    {
        // create the object
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
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
        var user = new User { Id = 1, Username = "testuser", Password = "password123" };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // get the username from the db repo
        var result = await _repository.GetUsernameById(1);

        // check if the result matches the username of the user
        Assert.Equal("testuser", result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
