/*
 * Smart Solar Microgrid Trading System
 * SE4040 - Enterprise Application Development
 *
 * File: UsersController.cs
 * Component: Web User and Account Management
 * Component Owner: WMDD Karunarathna (IT23145320)
 *
 * Purpose:
 * Exposes Backoffice-only management endpoints for Backoffice and Grid Operator users.
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSolarMicrogrid.Api.Common;
using SmartSolarMicrogrid.Api.DTOs;
using SmartSolarMicrogrid.Api.Models;
using SmartSolarMicrogrid.Api.Services;

namespace SmartSolarMicrogrid.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = nameof(UserRole.BACKOFFICE))]
public sealed class UsersController(
    IWebUserManagementService userService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<WebUserResponse>> Create(
        CreateWebUserRequest request,
        CancellationToken cancellationToken)
    {
        // Create an active Backoffice or assigned Grid Operator account.
        var result = await userService.CreateAsync(request, cancellationToken);
        return result.Status == WebUserManagementStatus.Success
            ? CreatedAtAction(nameof(GetById), new { userId = result.User!.UserId }, result.User)
            : MapResult(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebUserResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        // List only Backoffice and Grid Operator accounts.
        return Ok(await userService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<WebUserResponse>> GetById(
        string userId,
        CancellationToken cancellationToken)
    {
        // Retrieve one supported Web user through a safe response DTO.
        return MapResult(await userService.GetByIdAsync(userId, cancellationToken));
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<WebUserResponse>> Update(
        string userId,
        UpdateWebUserRequest request,
        CancellationToken cancellationToken)
    {
        // Update profile fields and an operator's validated station assignment.
        return MapResult(await userService.UpdateAsync(userId, request, cancellationToken));
    }

    [HttpPatch("{userId}/deactivate")]
    public async Task<ActionResult<WebUserResponse>> Deactivate(
        string userId,
        CancellationToken cancellationToken)
    {
        // Deactivate an active Web user without deleting the account.
        return MapResult(await userService.DeactivateAsync(userId, cancellationToken));
    }

    private ActionResult<WebUserResponse> MapResult(WebUserManagementResult result)
    {
        // Convert user-management outcomes to the established structured error contract.
        return result.Status switch
        {
            WebUserManagementStatus.Success => Ok(result.User),
            WebUserManagementStatus.InvalidUserId => BadRequest(Error(
                UserErrorCodes.InvalidUserId,
                "The user ID must be a valid MongoDB ObjectId.")),
            WebUserManagementStatus.UserNotFound => NotFound(Error(
                UserErrorCodes.UserNotFound,
                "The requested Web user was not found.")),
            WebUserManagementStatus.InvalidUserRole => BadRequest(Error(
                UserErrorCodes.InvalidUserRole,
                "Only BACKOFFICE and GRID_OPERATOR users may be created.")),
            WebUserManagementStatus.InvalidAccountState => Conflict(Error(
                UserErrorCodes.InvalidAccountState,
                "The requested account-state transition is not allowed.")),
            WebUserManagementStatus.InvalidStationId => BadRequest(Error(
                UserErrorCodes.InvalidStationId,
                "The station ID must be a valid MongoDB ObjectId.")),
            WebUserManagementStatus.StationNotFound => NotFound(Error(
                UserErrorCodes.StationNotFound,
                "The assigned station was not found.")),
            WebUserManagementStatus.EmailAlreadyExists => Conflict(Error(
                UserErrorCodes.EmailAlreadyExists,
                "A user with this email already exists.")),
            WebUserManagementStatus.OperatorStationRequired => BadRequest(Error(
                UserErrorCodes.OperatorStationRequired,
                "A Grid Operator must be assigned to a station.")),
            WebUserManagementStatus.ValidationError => BadRequest(Error(
                UserErrorCodes.ValidationError,
                result.ErrorMessage ?? "The supplied user data is invalid.")),
            _ => StatusCode(StatusCodes.Status500InternalServerError, Error(
                "SERVER_ERROR",
                "An unexpected error occurred."))
        };
    }

    private static ApiErrorResponse Error(string code, string message)
    {
        // Construct the shared structured API error response.
        return new ApiErrorResponse(code, message);
    }
}
