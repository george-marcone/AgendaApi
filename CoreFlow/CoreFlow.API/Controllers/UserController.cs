using System.Security.Claims;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Events;
using CoreFlow.Application.Queries;
using CoreFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Lists all registered users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<User>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        _logger.LogInformation("Listed {UserCount} users.", users.Length);
        return Ok(users);
    }

    /// <summary>
    /// Gets a user by id.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} was not found.", id);
            return NotFound();
        }

        _logger.LogInformation("User {UserId} retrieved.", id);
        return Ok(user);
    }

    /// <summary>
    /// Creates a user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(CreateUserCommand command, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedActor(out var actor))
        {
            return Unauthorized();
        }

        var id = await _mediator.Send(command with { Actor = actor }, cancellationToken);
        _logger.LogInformation("User {UserId} created.", id);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }

    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="id">User identifier from the route.</param>
    /// <param name="command">Updated user data.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedActor(out var actor))
        {
            return Unauthorized();
        }

        if (id != command.Id)
        {
            _logger.LogWarning("User update rejected because route id {RouteId} differs from body id {BodyId}.", id, command.Id);
            return BadRequest();
        }

        await _mediator.Send(command with { Actor = actor }, cancellationToken);
        _logger.LogInformation("User {UserId} updated.", id);
        return NoContent();
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <param name="request">Current and new password.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeOwnPassword(
        ChangeOwnPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            _logger.LogWarning("Password change rejected because the authenticated user id claim was invalid.");
            return Unauthorized();
        }

        var result = await _mediator.Send(
            new ChangeOwnPasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        if (result == ChangeOwnPasswordResult.UserNotFound)
        {
            _logger.LogWarning("Password change rejected because user {UserId} was not found.", userId);
            return Unauthorized();
        }

        if (result == ChangeOwnPasswordResult.InvalidCurrentPassword)
        {
            _logger.LogWarning("Password change rejected because current password was invalid for user {UserId}.", userId);
            return BadRequest(new { message = "Current password is invalid." });
        }

        _logger.LogInformation("User {UserId} changed their own password.", userId);
        return NoContent();
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedActor(out var actor))
        {
            return Unauthorized();
        }

        await _mediator.Send(new DeleteUserCommand(id, actor), cancellationToken);
        _logger.LogInformation("User {UserId} deleted.", id);
        return NoContent();
    }

    private bool TryGetAuthenticatedActor(out ContactEventActor actor)
    {
        actor = ContactEventActor.Unknown;

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            _logger.LogWarning("Request rejected because the authenticated user id claim was invalid.");
            return false;
        }

        actor = new ContactEventActor(
            userId,
            User.FindFirstValue(ClaimTypes.Name) ?? "Usuario autenticado",
            User.FindFirstValue(ClaimTypes.Email) ?? string.Empty);

        return true;
    }
}

public record ChangeOwnPasswordRequest(string CurrentPassword, string NewPassword);
