using System.Security.Claims;
using backend.ApplicationNEW.Dtos.Authentication;
using backend.ApplicationNEW.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.AccountManagement.User;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IUserService userService, 
        ILogger<AccountController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // Update the username for the currently authenticated user
    [HttpPut("username")]
    public async Task<ActionResult<UserDto>> UpdateUsername([FromBody] UpdateUsernameDto request)
    {
        _logger.LogInformation("[AccountController] UpdateUsername called");
        
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AccountController] Invalid ModelState for UpdateUsername: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("[AccountController] Extracted authUserId from token: {AuthUserId}", authUserId);
            
            // Check if authUserId is null or empty
            if (string.IsNullOrEmpty(authUserId))
            {
                _logger.LogWarning("[AccountController] Unable to get authenticated user ID");
                return Unauthorized("User not authenticated");
            }

            // Update username in the game database
            _logger.LogInformation("[AccountController] Attempting to update username for authUserId: {AuthUserId} to new username: {NewUsername}", authUserId, request.Username);
            var updatedUser = await _userService.UpdateUsername(authUserId, request);

            // no need to call UserManager as the UserService handles it now. -Ah

            _logger.LogInformation("[AccountController] Successfully updated username for user {UserId}", updatedUser.Id);
            return Ok(updatedUser);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[AccountController] Error updating username");
            return StatusCode(500, new { message = "An error occurred while updating username" });
        }
    }

    // Update the password for the currently authenticated user
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto request)
    {
        _logger.LogInformation("[AccountController] UpdatePassword called");
        
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AccountController] Invalid ModelState for UpdatePassword: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Check if authUserId is null or empty
            if (string.IsNullOrEmpty(authUserId))
            {
                _logger.LogWarning("[AccountController] Unable to get authenticated user ID");
                return Unauthorized("User not authenticated");
            }

            // Updates password in both databases
            await _userService.UpdatePassword(authUserId, request);

            _logger.LogInformation("[AccountController] Successfully updated password in both databases");
            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[AccountController] Error updating password");
            return StatusCode(500, new { message = "An error occurred while updating password" });
        }
    }

    // Delete the currently authenticated user's account
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        _logger.LogInformation("[AccountController] DeleteAccount called");

        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Check if authUserId is null or empty
            if (string.IsNullOrEmpty(authUserId))
            {
                _logger.LogWarning("[AccountController] Unable to get authenticated user ID for account deletion");
                return Unauthorized("User not authenticated");
            }

            _logger.LogInformation("[AccountController] Attempting to delete account for authUserId: {AuthUserId}", authUserId);

            // Delete from game database
            await _userService.DeleteAccount(authUserId);

            // same case here, Auth database user is also handled by the UserService now

            _logger.LogInformation("[AccountController] Successfully deleted account for authUserId: {AuthUserId}", authUserId);
            return Ok(new { message = "Account deleted successfully" });
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "[AccountController] User not found");
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[AccountController] Error deleting account");
            return StatusCode(500, new { message = "An error occurred while deleting account" });
        }
    }
}
