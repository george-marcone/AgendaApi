using System.Security.Claims;
using CoreFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <remarks>
    /// Use the returned access token as a Bearer token for protected endpoints.
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login rejected because email or password was empty.");
            return BadRequest(new { message = "Email and password are required." });
        }

        var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Invalid login attempt for email {Email}.", request.Email);
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwtTokenService.CreateToken(user);
        _logger.LogInformation("User {UserId} authenticated successfully.", user.Id);
        return Ok(new LoginResponse(
            token.AccessToken,
            "Bearer",
            token.ExpiresAt,
            new AuthenticatedUserResponse(user.Id, user.Name, user.Email)));
    }

    /// <summary>
    /// Validates the current Bearer token and returns the authenticated user.
    /// </summary>
    [HttpGet("authenticate")]
    [Authorize]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticateResponse>> Authenticate(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            _logger.LogWarning("Authenticated request had an invalid user id claim.");
            return Unauthorized();
        }

        var user = await _authService.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Authenticated user {UserId} was not found.", userId);
            return Unauthorized();
        }

        _logger.LogInformation("User {UserId} authentication status checked.", user.Id);
        return Ok(new AuthenticateResponse(true, new AuthenticatedUserResponse(user.Id, user.Name, user.Email)));
    }
}

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthenticatedUserResponse User);

public record AuthenticateResponse(bool Authenticated, AuthenticatedUserResponse User);

public record AuthenticatedUserResponse(Guid Id, string Name, string Email);
