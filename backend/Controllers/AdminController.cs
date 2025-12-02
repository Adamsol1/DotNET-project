using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Application.Interfaces.Services;
using backend.Application.Dtos;
using Serilog;
using System;

namespace backend.Controllers;

/*
Only admin can use this endpoint.
we check the users role, if it is not admin, we return a 403 Forbidden.
in the UI, we only show the admin button if the user is admin, by checking the role there to.
*/

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    //needs only userservice and logger as dependency, as UserManager is now handled by the UserService.
    private readonly IUserService _userService;
    private readonly ILogger<AdminController> _logger;

    //controller
    public AdminController(IUserService userService, ILogger<AdminController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // get all users - Admin
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        // debugging for console.
        _logger.LogInformation("Getting all users");

        try {

            // get all users from the user service.
            var users = await _userService.GetAllUsers();

            // return the users.
            // userService has passed the objects through DTO
            return Ok(users);
            
            // no need for error handling here 
            // as userservice &  repository handles the errors.
            // so it just bubbles up to the controller.
        }
        catch (KeyNotFoundException e)
        {
            // catch for when no users are found.
            _logger.LogWarning(e, "No users found");
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting all users");
            return StatusCode(500, "Internal server error");
        }
    }

    // get user by id 
    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {

        // debugging
        _logger.LogInformation("Getting user by id: {id}", id);

        try {

            // get the user by id from the user service.
            var user = await _userService.GetUserById(id);

            // return the user,
            return Ok(user);

        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "User with id {id} not found", id);
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting user by id: {id}", id);
            return StatusCode(500, "Internal server error");

        }
    }

    // update username - admin
    // its a put request because we are updating a particular user.
    [HttpPut("users/{id}/username")]
    public async Task<ActionResult<UserDto>> UpdateUserUsername(int id, [FromBody] UpdateUsernameDto request)
    {
        // debugging for console.
        _logger.LogInformation("Updating username for user with id: {id} in AdminController", id);

        // since this operation is ment to alter the database,
        // we check the model state to make sure the request is valid.

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for UpdateUserUsername: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try {

            var authUserId = await _userService.GetByAuthId(id);
            var user = await _userService.UpdateUsername(authUserId, request);

            // return the user,
            return Ok(user);

        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "User with id {id} not found", id);
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating username for user with id: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // update password - admin

    [HttpPut("users/{id}/password")]
    public async Task<IActionResult> UpdateUserPassword(int id, [FromBody] UpdatePasswordDto request)
    {
        // debugging for console.
        _logger.LogInformation("Updating password for user with id: {id} in AdminController", id);

        //checking the modal state
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for UpdateUserUsername: {@ModelState}", ModelState);
            return BadRequest(ModelState);
        }

        try {

            var authUserId = await _userService.GetByAuthId(id);
            await _userService.UpdatePassword(authUserId, request);

            // return a success message.
            return Ok(new { message = "Password updated successfully" });

        }
        catch (KeyNotFoundException e)
        {
            _logger.LogWarning(e, "User with id {id} not found", id);
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating password for user with id: {id}", id);
            return StatusCode(500, "Internal server error");
        }

    }

    //delete method for admin.
    // this endpoint just deletes the user so it has no need for DTO.
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // debugging
        _logger.LogInformation("Deleting user with id: {id} in AdminController", id);

        try {

            var authUserId = await _userService.GetByAuthId(id);
            
            if (string.IsNullOrEmpty(authUserId)) {
                _logger.LogWarning("AuthUserId not found for user with id {id}", id);
                return NotFound(new { message = "AuthUserId not found" });
            }

            await _userService.DeleteAccount(authUserId);

            // return a success message.
            return Ok(new { message = "User deleted successfully" });

        } catch (KeyNotFoundException e) {
            _logger.LogWarning(e, "User with id {id} not found", id);
            return NotFound(new { message = e.Message });
        }
        catch (Exception e) {
            _logger.LogError(e, "Error deleting user with id: {id}", id);
            return StatusCode(500, "Internal server error");
        }

        
    }

}
