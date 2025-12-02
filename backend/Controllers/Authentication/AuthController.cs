using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Application.Dtos.Authentication;
using backend.Application.Interfaces;
using backend.Domain.Models;
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
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<AuthUser> _userManager;
    private readonly SignInManager<AuthUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        IUserService userService, 
        ILogger<AuthController> logger,
        UserManager<AuthUser> userManager,
        SignInManager<AuthUser> signInManager,
        IConfiguration configuration)
    {
        _userService = userService;
        _logger = logger;
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
        // Log and validate incoming request
        _logger.LogInformation("[AuthController] Register called for username: {Username}", request?.Username);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AuthController] Invalid ModelState for register: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        // Attempt to contact service layer about the account registration
        try
        {
            // UserService now handles all UserManager operations and transaction management
            var gameUserDto = await _userService.RegisterAccount(request);
            
            _logger.LogInformation("[AuthController] Succesfully created account for {Username}", request.Username);
            
            return Ok(new {message = "Account created successfully", gameUserId = gameUserDto.Id});
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning("[AuthController] Registration failed: {Message}", e.Message);
            return BadRequest(new { message = e.Message });
        }
        catch(Exception e)
        {
            // log the error
            _logger.LogError(e, "[AuthController] Unexpected error occured while trying to create account for {Username}", request.Username);
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
        _logger.LogInformation("[AuthController] Login called for username: {Username}", request?.Username);
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AuthController] Invalid ModelState for login: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }
        // Try to use user service for login
        try
        {
            // UserService now handles authentication against AuthDb
            var user = await _userService.Login(request);
            
            if (user == null)
            {
                _logger.LogWarning("[AuthController] Login attempt failed for user : {@LoginUserDto}", request);
                return Unauthorized(new { message = "Incorrect username or password. Please try again."});
            }

            // Get AuthUser to generate JWT token
            var authUser = await _userManager.FindByNameAsync(request.Username);
            if (authUser == null)
            {
                _logger.LogWarning("[AuthController] AuthUser not found after successful login");
                return Unauthorized(new { message = "Authentication error. Please try again."});
            }

            _logger.LogInformation("[AuthController] Login attempt authorized for user : {@LoginUserDto}", request);
            var token = await GenerateJwtToken(authUser);
            _logger.LogInformation("[AuthController] GameUser found - Id: {UserId}, Username: {Username}", user.Id, user.Username);

            return Ok(new { token = token, userId = user.Id, username = user.Username });
        }
        catch (Exception e)
        {
            // log the error
            _logger.LogError(e, "[AuthController] Unexpected error occured while trying to login.");
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
        _logger.LogInformation("[AuthController] Logged out.");
        return Ok("Logged out successfully");

    }
    
    /// <summary>
    /// Get method for getting current user info
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("user")]
    [Authorize]
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


    private async Task<string> GenerateJwtToken(AuthUser user)
    {
        var JWTKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(JWTKey))
        {
            _logger.LogWarning($"JWT Key not set. Key: {JWTKey}");
            throw new InvalidOperationException("JWT Key not set. Key: {JWTKey}");
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
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );
        _logger.LogInformation("[AuthController] Generated JWT token for user @{UserName}.", user.UserName);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}