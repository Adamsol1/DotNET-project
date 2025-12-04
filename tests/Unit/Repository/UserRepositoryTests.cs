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

    public void Dispose()
    {
        _context.Dispose();
    }
}
