using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Application.Dtos.Authentication;
using backend.Application.Interfaces;
using backend.Domain.Models;
using backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace backend.Controllers.Authentication;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEntityFileLogger _entityLogger;
    private readonly UserManager<AuthUser> _userManager;
    private readonly SignInManager<AuthUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserService userService, 
        IEntityFileLogger entityFileLogger,
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        IConfiguration configuration)
    {
        _userService = userService;
        _entityLogger = entityFileLogger;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
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
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Attempt to contact service layer about the account registration
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request cannot be null" });

            // UserService now handles all UserManager operations and transaction management
            var gameUserDto = await _userService.RegisterAccount(request);
            
            // Log the successful registration
            await _entityLogger.LogAsync(
                "register account successful",
                gameUserDto,
                LogCategories.Authentication
            );
            
            return Ok(new {message = "Account created successfully", gameUserId = gameUserDto.Id});
        }
        catch (InvalidOperationException e)
        {
            // Log the registration failure
            await _entityLogger.LogAsync(
                "register account error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Authentication
            );
            return BadRequest(new { message = e.Message });
        }
        catch(Exception e )
        {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "register account error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Authentication
            );
            // return the error
            return BadRequest(new { message = "Unexpected error occured while creating account." });
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
    public async Task<ActionResult<LoginUserDto>> Login([FromBody] LoginUserDto request)
    {
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        // Try to use user service for login
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request cannot be null" });

            // UserService now handles authentication against AuthDb
            var user = await _userService.Login(request);
            
            if (user == null)
            {
                // Log failed login attempt
                await _entityLogger.LogAsync(
                    "login error invalid credentials",
                    new
                    {
                        Timestamp = DateTime.UtcNow
                    },
                    LogCategories.Authentication
                );
                return BadRequest(new { message = "Incorrect username or password. Please try again."});
            }

            // Get AuthUser to generate JWT token
            var authUser = await _userManager.FindByNameAsync(request.Username);
            if (authUser == null)
            {
                // Log authentication error
                await _entityLogger.LogAsync(
                    "login error auth user not found",
                    new
                    {
                        Timestamp = DateTime.UtcNow
                    },
                    LogCategories.Authentication
                );
                return Unauthorized(new { message = "Authentication error. Please try again."});
            }

            var token = await GenerateJwtToken(authUser);
            
            // Log successful login
            await _entityLogger.LogAsync(
                "login successful",
                new
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Authentication
            );

            return Ok(new { token = token, userId = user.Id, username = user.Username });
        }
        catch (Exception e)
        {
            // Log unexpected error
            await _entityLogger.LogAsync(
                "login error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Authentication
            );
            // return the error
            return BadRequest(new { message = "Unexpected error occured while trying to login." });
        }
    }
    


    /// <summary>
        /// Post method for logout. 
        /// </summary>
        /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        
        // Log successful logout
        await _entityLogger.LogAsync(
            "logout successful",
            new
            {
                Timestamp = DateTime.UtcNow
            },
            LogCategories.Authentication
        );
        
        return Ok("Logged out successfully");
    }
    


    private async Task<string> GenerateJwtToken(AuthUser user)
    {
        var JWTKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(JWTKey))
        {
            throw new InvalidOperationException("JWT Key not set.");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var accountRoles = await _userManager.GetRolesAsync(user);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        };
        foreach (var role in accountRoles)
        {
            claims = claims.Append(new Claim(ClaimTypes.Role, role)).ToArray();
        }
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddSeconds(20),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}