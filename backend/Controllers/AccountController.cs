using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using backend.Domain.Models;

namespace backend.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;
    private readonly UserManager<AuthUser> _userManager;

    public AccountController(
        IUserService userService, 
        ILogger<AccountController> logger,
        UserManager<AuthUser> userManager)
    {
        _userService = userService;
        _logger = logger;
        _userManager = userManager;
    }

    // Update the username for the currently authenticated user
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateUsername([FromBody] UpdateUsernameDto request)
    {
        _logger.LogInformation("[AccountController] UpdateUsername called");
        
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
            
            if (string.IsNullOrEmpty(authUserId))
            {
                _logger.LogWarning("[AccountController] Unable to get authenticated user ID");
                return Unauthorized("User not authenticated");
            }

            // Update username in the game database
            _logger.LogInformation("[AccountController] Attempting to update username for authUserId: {AuthUserId} to new username: {NewUsername}", authUserId, request.Username);
            var updatedUser = await _userService.UpdateUsername(authUserId, request);
            
            // Update username in Identity (AuthUser)
            var authUser = await _userManager.FindByIdAsync(authUserId);
            if (authUser != null)
            {
                authUser.UserName = request.Username;
                var result = await _userManager.UpdateAsync(authUser);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("[AccountController] Failed to update AuthUser username: {@Errors}", result.Errors);
                    return BadRequest(new { message = "Failed to update authentication username" });
                }
            }

            _logger.LogInformation("[AccountController] Successfully updated username for user {UserId}", updatedUser.Id);
            return Ok(updatedUser);
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "[AccountController] Username already exists");
            return BadRequest(new { message = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "[AccountController] User not found");
            return NotFound(new { message = e.Message });
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
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AccountController] Invalid ModelState for UpdatePassword: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(authUserId))
            {
                _logger.LogWarning("[AccountController] Unable to get authenticated user ID");
                return Unauthorized("User not authenticated");
            }

            // Update password in Identity (AuthUser)
            var authUser = await _userManager.FindByIdAsync(authUserId);
            if (authUser == null)
            {
                _logger.LogWarning("[AccountController] AuthUser not found for ID {AuthUserId}", authUserId);
                return NotFound(new { message = "User not found" });
            }

            // Remove old password and add new one (since we don't have the old password)
            var token = await _userManager.GeneratePasswordResetTokenAsync(authUser);
            var result = await _userManager.ResetPasswordAsync(authUser, token, request.NewPassword);

            if (!result.Succeeded)
            {
                _logger.LogWarning("[AccountController] Failed to update password: {@Errors}", result.Errors);
                return BadRequest(new { message = "Failed to update password", errors = result.Errors });
            }

            // Update password in the game database
            await _userService.UpdatePassword(authUserId, request);

            _logger.LogInformation("[AccountController] Successfully updated password for AuthUserId {AuthUserId}", authUserId);
            return Ok(new { message = "Password updated successfully" });
        }
        catch (InvalidOperationException e)
        {
            _logger.LogWarning(e, "[AccountController] Password validation failed");
            return BadRequest(new { message = e.Message });
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "[AccountController] User not found");
            return NotFound(new { message = e.Message });
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
            
            if (string.IsNullOrEmpty(authUserId))
            {
                _logger.LogWarning("[AccountController] Unable to get authenticated user ID for account deletion");
                return Unauthorized("User not authenticated");
            }

            _logger.LogInformation("[AccountController] Attempting to delete account for authUserId: {AuthUserId}", authUserId);

            // Delete from game database
            await _userService.DeleteAccount(authUserId);

            // Delete from Identity (AuthUser)
            var authUser = await _userManager.FindByIdAsync(authUserId);
            if (authUser != null)
            {
                var result = await _userManager.DeleteAsync(authUser);
                
                if (!result.Succeeded)
                {
                    _logger.LogWarning("[AccountController] Failed to delete AuthUser: {@Errors}", result.Errors);
                    return BadRequest(new { message = "Failed to delete authentication account" });
                }
            }

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
