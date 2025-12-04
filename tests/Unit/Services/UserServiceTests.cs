using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using backend.Application.Dtos.Authentication;
using backend.Application.Services.Authentication;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using backend.Infrastructure.Repositories.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace tests;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEntityFileLogger> _mockLogger;
    private readonly Mock<UserManager<AuthUser>> _mockUserManager;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserService _service;

    // constructor registers and sets up a mock of the services that the
    // userservices relies on to make the tests work

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<IEntityFileLogger>();
        _mockUserRepository = new Mock<IUserRepository>();
        
        // stores the authuser in a mock database, for the UserManager from identityFramework
        var store = Mock.Of<IUserStore<AuthUser>>();
        _mockUserManager = new Mock<UserManager<AuthUser>>(
            store, 
            Mock.Of<IOptions<IdentityOptions>>(), 
            Mock.Of<IPasswordHasher<AuthUser>>(), 
            Array.Empty<IUserValidator<AuthUser>>(), 
            Array.Empty<IPasswordValidator<AuthUser>>(), 
            Mock.Of<ILookupNormalizer>(), 
            Mock.Of<IdentityErrorDescriber>(), 
            Mock.Of<IServiceProvider>(), 
            Mock.Of<ILogger<UserManager<AuthUser>>>());
        
        // Setup UnitOfWork to return UserRepository
        // creates a UnitOfWork mock that returns for the UserRepository
        _mockUnitOfWork.Setup(x => x.UserRepository).Returns(_mockUserRepository.Object);
        
        // register the test service with the mock services.
        _service = new UserService(
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockUserManager.Object
        );
    }

    // positive test to create user sucessfully
    [Fact]
    public async Task RegisterAccount_ShouldCreateUserSuccessfully()
    {
        // Arrange test from the Data transfer object
        // has the validation method and defined parameters.
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Password = "TestPass123!"
        };

        // reusable method to setup the transaction and create a sucessfull user
        // to avoid having to repeat the same code bocks everytime.
        createUowTransactions();
        createSuccessUser("testuser", "TestPass123!");

        // call the service method to create the user based on the dto.
        var result = await _service.RegisterAccount(registerDto);

        // check if the user is created successfully
        //notnull makes sure user is not null, returns its id and matches to the created user.
        //aswell as the name. and verifies that it is created in the databases.
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("testuser", result.Username);
        _mockUserRepository.Verify(x => x.GetUserByUsername("testuser"), Times.Once);
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AuthUser>(), "TestPass123!"), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    // negative test path for when the username already exists
    [Fact]
    public async Task RegisterAccount_ShouldThrowException_WhenUsernameAlreadyExists()
    {
        // create a dto object that is used to register the user
        var registerDto = new RegisterUserDto
        {
            Username = "exists",
            Password = "TestPass123!"
        };

        // create a mock user that already exists in the database
        var existingUser = new User { Id = 1, Username = "exists" };

        // create a mock repository that returns an existing user with the same name.
        _mockUserRepository.Setup(x => x.GetUserByUsername("exists"))
            .ReturnsAsync(existingUser);
        createUowTransactions();

        // validates that the user is not created by checking if exception is thrown. and transaction is rolled back.
        // aswell as the user manager is not called to create the user.
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAccount(registerDto));
        _mockUnitOfWork.Verify(x => x.RollBackAsync(), Times.AtLeastOnce);
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<AuthUser>(), It.IsAny<string>()), Times.Never);
    }

    // negative test path for when the user creation fails
    [Fact]
    public async Task RegisterAccount_ShouldThrowException_WhenUserCreationFails()
    {
        // data transfer object
        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Password = "WeakPass"
        };

        // create an error array that is returned by the user manager when the user creation fails.
        var errors = new[] { new IdentityError { Description = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character" } };

        // mock setup with the user to check that the user is not created. aswell as in the userManager
        _mockUserRepository.Setup(x => x.GetUserByUsername("testuser"))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), "WeakPass"))
            .ReturnsAsync(IdentityResult.Failed(errors));
        createUowTransactions();

        // catches exception to check that the it is thrown and checking that we did rollback
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RegisterAccount(registerDto));
        Assert.Contains("Unable to create account", exception.Message);
        _mockUnitOfWork.Verify(x => x.RollBackAsync(), Times.AtLeastOnce);
    }


    [Fact]
    public async Task UpdateUsername_ShouldUpdateUsernameSuccessfully()
    {
        // set user id and update dto
        var authUserId = "auth-123";
        var updateDto = new UpdateUsernameDto { Username = "newusername" };
        // create a mock user that already exists in the database
        var existingUser = new User { Id = 1, Username = "oldusername", AuthUserId = authUserId };
        // create a mock auth user that already exists in the database
        var authUser = new AuthUser { Id = authUserId, UserName = "oldusername" };

        // create a mock repository that returns an existing user with the same name.
        // create a mock repository that returns an existing user with the same name.
        createUowTransactions();
        createSuccessUsernameUpdate(authUserId, "newusername", existingUser, authUser);

        // call the service method to update the username based on the dto.
        var result = await _service.UpdateUsername(authUserId, updateDto);

        // check if the username is updated successfully
        //notnull makes sure user is not null, returns its id and matches to the created user.
        //aswell as the name. and verifies that it is created in the databases.
        Assert.NotNull(result);
        Assert.Equal("newusername", existingUser.Username);
        _mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<AuthUser>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateUsername_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // set user id and update dto
        var authUserId = "nonexistent";
        var updateDto = new UpdateUsernameDto { Username = "newusername" };

        _mockUserRepository.Setup(x => x.GetByAuthId(authUserId))
            .ReturnsAsync((User?)null);
        createUowTransactions();

        // catches exception to check that the it is thrown and checking that we did rollback
        await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() => _service.UpdateUsername(authUserId, updateDto));
        // verifies that the rollback is called at least once
        _mockUnitOfWork.Verify(x => x.RollBackAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateUsername_ShouldThrowInvalidOperationException_WhenUsernameAlreadyTaken()
    {
        // set user id and update dto
        var authUserId = "auth-123";
        var updateDto = new UpdateUsernameDto { Username = "takenusername" };
        // create two mock users that already exists in the database
        var existingUser = new User { Id = 1, Username = "oldusername", AuthUserId = authUserId };
        var takenUser = new User { Id = 2, Username = "takenusername", AuthUserId = "auth-456" };

        // create a mock repository that returns an existing user with the same auth user id.
        _mockUserRepository.Setup(x => x.GetByAuthId(authUserId))
            .ReturnsAsync(existingUser);
        // create a mock repository that returns an existing user with the same username.
        _mockUserRepository.Setup(x => x.GetUserByUsername("takenusername"))
            .ReturnsAsync(takenUser);
        // setup the transaction and create a sucessfull user
        createUowTransactions();

        // catches exception to check that the it is thrown and checking that we did rollback
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateUsername(authUserId, updateDto));
        Assert.Contains("Username already taken", exception.Message);
        _mockUnitOfWork.Verify(x => x.RollBackAsync(), Times.AtLeastOnce);
    }


    [Fact]
    public async Task Login_ShouldReturnUserDto_WhenCredentialsAreValid()
    {
        // create a login dto object that is used to login the user
        var loginDto = new LoginUserDto
        {
            Username = "testuser",
            Password = "TestPass123!"
        };

        // create a mock user that already exists in the database
        var gameUser = new User { Id = 1, Username = "testuser", AuthUserId = "auth-123" };
        // create a mock repository that returns an existing user with the same name.
        createSuccessLogin("testuser", "TestPass123!", "auth-123", gameUser);

        // call the service method to login the user based on the dto.
        var result = await _service.Login(loginDto);

        // check if the user is logged in successfully
        //notnull makes sure user is not null, returns its id and matches to the created user.
        //aswell as the name. and verifies that it is created in the databases.
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenUsernameDoesNotExist()
    {
        // create a login dto object that is used to login the user
        var loginDto = new LoginUserDto
        {
            Username = "nonexistent",
            Password = "TestPass123!"
        };

        // create a mock repository that returns an existing user with the same name.
        _mockUserManager.Setup(x => x.FindByNameAsync("nonexistent"))
            .ReturnsAsync((AuthUser?)null);

        // call the service method to login the user based on the dto.
        var result = await _service.Login(loginDto);

        // check if the user is not logged in successfully
        //notnull makes sure user is not null, returns its id and matches to the created user.
        //aswell as the name. and verifies that it is created in the databases.
        Assert.Null(result);
        // verifies that the password check is not called
        _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<AuthUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // create a login dto object that is used to login the user
        var loginDto = new LoginUserDto
        {
            Username = "testuser",
            Password = "WrongPassword"
        };

        // create a mock auth user that already exists in the database
        var authUser = new AuthUser { Id = "auth-123", UserName = "testuser" };

        // create a mock repository that returns an existing user with the same name.
        _mockUserManager.Setup(x => x.FindByNameAsync("testuser"))
            .ReturnsAsync(authUser);
        // create a mock repository that returns an existing user with the same name.
        _mockUserManager.Setup(x => x.CheckPasswordAsync(authUser, "WrongPassword"))
            .ReturnsAsync(false);

        // call the service method to login the user based on the dto.
        var result = await _service.Login(loginDto);

        // check if the user is not logged in successfully
        //notnull makes sure user is not null, returns its id and matches to the created user.
        //aswell as the name. and verifies that it is created in the databases.
        Assert.Null(result);
        // verifies that the get by auth id is not called
        _mockUserRepository.Verify(x => x.GetByAuthId(It.IsAny<string>()), Times.Never);
    }


    [Fact]
    public async Task GetUserById_ShouldReturnUserDto()
    {
        // set user id and create a mock user that already exists in the database
        var userId = 1;
        var user = new User { Id = userId, Username = "testuser", AuthUserId = "auth-123" };

        // create a mock repository that returns an existing user with the same id.
        _mockUserRepository.Setup(x => x.GetById(userId))
            .ReturnsAsync(user);

        // call the service method to get the user by id.
        var result = await _service.GetUserById(userId);

        // check if the user is returned successfully
        //notnull makes sure user is not null, returns its id and matches to the created user.
        //aswell as the name. and verifies that it is created in the databases.
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetUserById_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // set user id and create a mock user that already exists in the database
        var userId = 999;

        // create a mock repository that returns an existing user with the same id.
        _mockUserRepository.Setup(x => x.GetById(userId))
            .ThrowsAsync(new System.Collections.Generic.KeyNotFoundException($"Entity of type User with id {userId} was not found."));

        // catches exception to check that the it is thrown and checking that we did rollback
        await Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() => _service.GetUserById(userId));
    }

    /// <summary>
    /// Sets up common UnitOfWork transaction methods (BeginAsync, CommitAsync, RollBackAsync, SaveAsync)
    /// </summary>
    private void createUowTransactions()
    {
        _mockUnitOfWork.Setup(x => x.BeginAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.RollBackAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);
    }

    /// <summary>
    /// Sets up successful user creation flow for registration tests
    /// </summary>
    private void createSuccessUser(string username, string password, int userId = 1)
    {
        _mockUserRepository.Setup(x => x.GetUserByUsername(username))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AuthUser>(), password))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<AuthUser>(), "player"))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserRepository.Setup(x => x.Create(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = userId; return u; });
    }

    /// <summary>
    /// Sets up successful username update flow
    /// </summary>
    private void createSuccessUsernameUpdate(string authUserId, string newUsername, User existingUser, AuthUser authUser)
    {
        _mockUserRepository.Setup(x => x.GetByAuthId(authUserId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.GetUserByUsername(newUsername))
            .ReturnsAsync((User?)null);
        _mockUserManager.Setup(x => x.FindByIdAsync(authUserId))
            .ReturnsAsync(authUser);
        _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<AuthUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _mockUserRepository.Setup(x => x.Update(It.IsAny<User>()))
            .ReturnsAsync(existingUser);
    }

    /// <summary>
    /// Sets up successful login flow
    /// </summary>
    private void createSuccessLogin(string username, string password, string authUserId, User gameUser)
    {
        var authUser = new AuthUser { Id = authUserId, UserName = username };
        _mockUserManager.Setup(x => x.FindByNameAsync(username))
            .ReturnsAsync(authUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(authUser, password))
            .ReturnsAsync(true);
        _mockUserRepository.Setup(x => x.GetByAuthId(authUserId))
            .ReturnsAsync(gameUser);
    }

}
