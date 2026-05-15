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

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required." });
        }

        var user = await _authService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwtTokenService.CreateToken(user);
        return Ok(new LoginResponse(
            token.AccessToken,
            "Bearer",
            token.ExpiresAt,
            new AuthenticatedUserResponse(user.Id, user.Name, user.Email)));
    }

    [HttpGet("authenticate")]
    [Authorize]
    public async Task<ActionResult<AuthenticateResponse>> Authenticate(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

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
