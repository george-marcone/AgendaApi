using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(AuthUser user);
}

public record JwtTokenResult(string AccessToken, DateTimeOffset ExpiresAt);
