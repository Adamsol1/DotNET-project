using System.Security.Claims;
using backend.Application.Dtos.Authentication;
using backend.Application.Interfaces;
using backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.AccountManagement.User;

/// <summary>
/// Controller for account management for a given user. It allows for updating username, password and deleting the specific account.
/// This requires authentication with a valid JWT token to be able to perform these operations.
/// </summary>

[ApiController]
[Route("api/account")] // Default route for account management
[Authorize] // Check for a valid jwt token
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IEntityFileLogger _entityLogger;

    public AccountController(
        IUserService userService,
        IEntityFileLogger entityLogger)
    {
        _userService = userService;
        _entityLogger = entityLogger;
    }

    /// <summary>
    /// Method used to update the username for the current user. 
    /// </summary>
    /// <param name="request">Updated username</param>
    /// <returns>The updated user information</returns>
    [HttpPut("username")]
    public async Task<ActionResult<UserDto>> UpdateUsername([FromBody] UpdateUsernameDto request)
    {
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if authUserId is null or empty
            if (string.IsNullOrEmpty(authUserId))
            {
                return Unauthorized("User not authenticated");
            }

            var updatedUser = await _userService.UpdateUsername(authUserId, request);

            // Log the successful HTTP response
            await _entityLogger.LogAsync(
                "update username successful",
                updatedUser,
                LogCategories.Account
            );

            return Ok(updatedUser);
        }
        catch (InvalidOperationException e)
        {
            // Log the error
            await _entityLogger.LogAsync(
                "update username error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.AccountManagement.Username
            );
            return BadRequest(new { message = e.Message });
        }
        catch (Exception e)
        {
            // Log the error
            await _entityLogger.LogAsync(
                "update username error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Account
            );
            return StatusCode(500, new { message = "An error occurred while updating username" });
        }
    }

    // Update the password for the currently authenticated user
    /// <summary>
    /// Method used to update the password for the current user.
    /// It takes in a new password, and attempts to update it. If successful, it returns a success message.
    /// </summary>
    /// <param name="request">The request with the new password</param>
    /// <returns>The success status</returns>
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto request)
    {
        // Validate the incoming request
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if authUserId is null or empty
            if (string.IsNullOrEmpty(authUserId))
            {
                return Unauthorized("User not authenticated");
            }

            // Updates password in both databases
            await _userService.UpdatePassword(authUserId, request);

            // Log the successful HTTP response
            await _entityLogger.LogAsync(
                "update password successful",
                new { Timestamp = DateTime.UtcNow },
                LogCategories.Account
            );

            return Ok(new { message = "Password updated successfully" });
        }
        catch (Exception e)
        {
            // Log the error
            await _entityLogger.LogAsync(
                "update password error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Account
            );
            return StatusCode(500, new { message = "An error occurred while updating password" });
        }
    }

    // Delete the currently authenticated user's account
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            // Get the authenticated user's ID from the JWT token
            var authUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if authUserId is null or empty
            if (string.IsNullOrEmpty(authUserId))
            {
                return Unauthorized("User not authenticated");
            }

            // Delete from game database and auth database (handled by UserService)
            await _userService.DeleteAccount(authUserId);

            // Log the successful HTTP response
            await _entityLogger.LogAsync(
                "delete account successful",
                new { Timestamp = DateTime.UtcNow },
                LogCategories.Account
            );

            return Ok(new { message = "Account deleted successfully" });
        }
        catch (KeyNotFoundException e)
        {
            // Log the error and return a 404 Not Found response
            await _entityLogger.LogAsync(
                "delete account error not found",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Account
            );
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            // Log the error and return a 500 Internal Server Error response
            await _entityLogger.LogAsync(
                "delete account error",
                new
                {
                    Error = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Account
            );
            return StatusCode(500, new { message = "An error occurred while deleting account" });
        }
    }
}
