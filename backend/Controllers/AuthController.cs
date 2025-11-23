using Microsoft.AspNetCore.Mvc;
using backend.Application.Interfaces.Repositories;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using backend.ViewModels;

namespace backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    /// <summary>
    /// This controller method will contact the service layer about registering a user with given information.
    /// If sucessfull the method will inform the user about the account registraion.
    /// If unsuccesfull the method will return an error to the user explaining why it failed. 
    /// </summary>
    /// <param name="registerUserDto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto request)
    {
        // Attempt to contact service layer about the account registration
        try
        {
            //Await the answer given by the service layer. 
            var userDto = await _userService.RegisterAccount(request);
            //If the service layer was unable to create the account inform the user. 
            if (userDto == null)
            {
                // TODO : Inform user about the error
                return BadRequest("Failed to create account. Username may already exist.");
                
            }

            //If account registration succesfull the user will be informed and redirected to the login page. 
            // TODO : Inform user about account registration
            return Ok(userDto);

        //If unable to contact service layer error will be given
        } catch (InvalidOperationException e)
        {
            // log the error
            _logger.LogError(e, "User with username already exists.");
            // return the error
            return BadRequest(new { message = e.Message });
        }
        catch(Exception e)
        {
            // log the error
            _logger.LogError(e, "[AuthController] Failed to create account. Username may already exist.");
            // return the error
            return BadRequest(new { message = "Failed to create account. Please try again." });

        }
    }


    /// <summary>
    /// Method for the controller to contact the service layer to check if login attempt is valid.
    /// If the login attempt is valid the user will be redirected to the home page of the website.
    /// If not valid the user will be given an error explaining why they were unable to login. 
    /// </summary>
    /// <param name="loginUserDto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginUserDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        // Try to use user service for login
        try
        {
            var userDto = await _userService.Login(request);

            //Checks if the user service found a user with given username and password
            if (userDto == null)
            {
                // return the error
                return Unauthorized("Invalid username or password.");
            }

            // If the service returns a valid userDTO the webpage will navigate to the Home page. 
            return Ok(userDto);
        }
            //If unable to use the service the user will be given an error message. 
            catch (Exception e)
            {
                // log the error
                _logger.LogError(e, "[AuthController] Failed to login. Invalid username or password.");
                // return the error
                return BadRequest("Failed to login. Invalid username or password.");
            }
        }
    


    /// <summary>
        /// Post method for logout. 
        /// </summary>
        /// <returns>Success message</returns>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // return the success message since we dont use JWT tokens yet
        return Ok(new { message = "Logged out successfully" });

    }
    
    /// <summary>
    /// Get method for getting current user info
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("user")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // For now, return the admin user since we're using admin credentials
        // In JWT implementation, this would extract user from token
        return Ok(new UserDto 
        { 
            Id = 1, 
            Username = "admin", 
        });
    }

    // COMMENTED OUT FOR REACT FRONTEND - These were MVC view methods
    /*
    /// <summary>
    /// Get method for displaying the login page
    /// </summary>
    /// <returns>The view of the loginpage</returns>
    [HttpGet("login")]
    public IActionResult login()
    {
        return View(new LogInViewModel());
    }

    ///<summary
    /// Get method for displaying the register page
    /// </summary>
    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }
    */

    
}