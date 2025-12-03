using backend.Application.Dtos.Authentication;
using backend.Application.Interfaces;
using backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.AccountManagement.Admin;


/// <summary>
/// Admincontroller is the controller that handles all admin related actions.
/// The controller handles requests such as getting all users, getting a speicific user by their id, update a account username or password, and deleting a user account.
/// To enforce that only an admin can use these actions, the users role is checked using Authorize. Only users with the role "admin" are allowed access. 
/// The button used to access the admin functions will only show to users with the role "admin" in the UI.
/// If a non admin user tries to access the endpoint, a 403 Forbidden is returned.
/// </summary>

[ApiController]
[Route("api/admin")] //Base route for the admin controller.
[Authorize(Roles = "admin")] //Only user with admin role have access.
public class AdminController : ControllerBase
{
    //needs only userservice and logger as dependency, as UserManager is now handled by the UserService.
    //Logger used for logging admin actions. 
    private readonly IEntityFileLogger _entityLogger;
    private readonly IUserService _userService;


    //controller
    public AdminController(IUserService userService, IEntityFileLogger entityFileLogger)
    {
        _userService = userService;
        _entityLogger = entityFileLogger;
    }


    /// <summary>
    /// Method to get all the users in the system, only used by admins. 
    /// The method calls the user service to get all the users. If successful it will return a list of UserDto objects.
    /// </summary>
    /// <returns>A list of UserDTOs.</returns>

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        try
        {

            // get all users from the user service.
            var users = await _userService.GetAllUsers();

            // return the users.
            // userService has passed the objects through DTO.
            //returns HTTP 200 OK and the list of users. 
            return Ok(users);

            // no need for error handling here 
            // as userservice &  repository handles the errors.
            // so it just bubbles up to the controller.
        }
        catch (KeyNotFoundException e)
        {
            // catch for when no users are found. Logs the incident. 
            await _entityLogger.LogAsync(
                "get users error not found",
                new
                {
                    Reason = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserRetrieval,
                "UserRetrievalErrorLog");

            //Return the not found response to the client with the error message.
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            //Catch for all unexpected errors. Logs the incident.
            await _entityLogger.LogAsync(
                "get users error",
                new
                {
                    Reason = e.Message,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserRetrieval,
                "UserRetrievalErrorLog");

            //Return a generic internal server error to client. 
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Method to get a specific user by their id, only used by admins. 
    /// The method calls the user service to get the user by id. If successful it will return a UserDto object.
    /// </summary>
    /// <param name="id">Identifier of the user</param>
    /// <returns>A user dto of the given user</returns>

    [HttpGet("users/{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        try
        {

            // get the user by id from the user service.
            var user = await _userService.GetUserById(id);

            // return HTTP 200 OK and the user to the client. 
            return Ok(user);

        }
        catch (KeyNotFoundException e)
        {
            //Logs error if the user with the given id was not found.
            await _entityLogger.LogAsync(
                "get user by id error not found",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserRetrieval,
                "UserRetrievalErrorLog");

            //Return the not found response to the client with the error message.
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            //Logs any unexpected error occurring.
            await _entityLogger.LogAsync(
                "get user by id error",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserRetrieval,
                "UserRetrievalErrorLog");

            //Return a generic internal server error to client.
            return StatusCode(500, "Internal server error");

        }
    }

    
    /// <summary>
    /// Method to update a users username by their id. Only accessable by admins. 
    /// The method calls the user service to update the username. If successful it will return the updated UserDto object.
    /// </summary>
    /// <param name="id">The identifier of the user</param>
    /// <param name="request">The updated Userdto</param>
    /// <returns></returns>
    [HttpPut("users/{id}/username")]
    public async Task<ActionResult<UserDto>> UpdateUserUsername(int id, [FromBody] UpdateUsernameDto request)
    {
        // since this operation is ment to alter the database,
        // we check the model state to make sure the request is valid.

        if (!ModelState.IsValid)
        {
            // return bad request if the model state is invalid.
            return BadRequest(ModelState);
        }

        try {
            //Get the auth user id from the service.
            var authUserId = await _userService.GetByAuthId(id);
            // Attempt to update the username with service. 
            var user = await _userService.UpdateUsername(authUserId, request);

            // If successful, log and return the updated user dto and HTTP 200 OK.
            await _entityLogger.LogAsync(
                "update username successful",
                user,
                LogCategories.Administration.UserModification,
                "UserUpdateLog");

            return Ok(user);

        }
        catch (KeyNotFoundException e)
        {
            //Logs if the user with given id is not found. 
            await _entityLogger.LogAsync(
                "update username error not found",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserRetrieval,
                "UserRetrievalErrorLog");
            //Return the not found response to the client with the error message.
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            //Logs if the username already exists.
            await _entityLogger.LogAsync(
                "update username error",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserModification,
                "UserUpdateErrorLog");
            //Return the error message to the client.
            return BadRequest(new { message = e.Message });
        }
        catch (Exception e)
        {
            //Logs any unexpected error occurring.
            await _entityLogger.LogAsync(
                "update username error",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserModification,
                "UserUpdateErrorLog");
            //returns a general error message informing about the internal server error.
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // update password - admin
    /// <summary>
    /// Method used to update a users password by id. Only accessable by admins. 
    /// The method calls the user service to update the password. If successful it will return a success message.
    /// </summary>
    /// <param name="id">Identification of the user being updated</param>
    /// <param name="request">The updated password information</param>
    /// <returns>Success message</returns>
    [HttpPut("users/{id}/password")]
    public async Task<IActionResult> UpdateUserPassword(int id, [FromBody] UpdatePasswordDto request)
    {
        //checking the modal state
        if (!ModelState.IsValid)
        {
            //If not valid, return bad request.
            return BadRequest(ModelState);
        }

        try
        {
            //Attempts to retrieve the auth user id and update the password. 
            var authUserId = await _userService.GetByAuthId(id);
            //Attempts to update the password with service.
            await _userService.UpdatePassword(authUserId, request);

            // Log success and return a success message.
            await _entityLogger.LogAsync(
                "update password successful",
                new
                {
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserModification,
                "UserUpdateLog");

            return Ok(new { message = "Password updated successfully" });

        }
        catch (KeyNotFoundException e)
        {
            //Logs if the user with given id is not found and returns a bad request.
            await _entityLogger.LogAsync(
                "update password error not found",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserRetrieval,
                "UserRetrievalErrorLog");
            return NotFound(new { message = e.Message });
        }
        catch (Exception e)
        {
            //Logs any unexpected error occurring and returns a general internal server error.

            await _entityLogger.LogAsync(
                "update password error",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserModification,
                "UserUpdateErrorLog");
            return StatusCode(500, "Internal server error");
        }

    }

    //delete method for admin.
    // this endpoint just deletes the user so it has no need for DTO.
    /// <summary>
    /// Method used to delete a user by their id. Only accessable by admins. 
    /// The method calls the user service to delete the user. If successful it will return a success message.
    /// </summary>
    /// <param name="id">The id of the user to delete.</param>
    /// <returns>Success message if user is deleted.</returns>
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try {

            //Attempts to retrieve the auth user id and delete the account
            var authUserId = await _userService.GetByAuthId(id);
            //If the auth user with given id is not found, return error message. 
            if (string.IsNullOrEmpty(authUserId)) {
                await _entityLogger.LogAsync(
                    "delete user error not found",
                    new
                    {
                        UserId = id,
                        Timestamp = DateTime.UtcNow
                    },
                    LogCategories.Administration.UserDeletion,
                    "UserDeletionErrorLog");
                return NotFound(new { message = "AuthUserId not found" });
            }
            //Attempts to delete the user with service.
            await _userService.DeleteAccount(authUserId);

            // Log success and return a success message.
            await _entityLogger.LogAsync(
                "delete user successful",
                new
                {
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserDeletion,
                "UserDeletionLog");

            return Ok(new { message = "User deleted successfully" });

        } catch (KeyNotFoundException e) {
            //Logs if the user with given id is not found and returns error message.
            await _entityLogger.LogAsync(
                "delete user error not found",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserDeletion,
                "UserDeletionErrorLog");
            return NotFound(new { message = e.Message });
        }
        catch (Exception e) {
            await _entityLogger.LogAsync(
                "delete user error",
                new
                {
                    Reason = e.Message,
                    UserId = id,
                    Timestamp = DateTime.UtcNow
                },
                LogCategories.Administration.UserDeletion,
                "UserDeletionErrorLog");
            return StatusCode(500, "Internal server error");
        }

        
    }

}
