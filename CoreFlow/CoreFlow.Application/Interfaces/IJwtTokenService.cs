using CoreFlow.Domain.Entities;

namespace CoreFlow.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(User user);
}

public record JwtTokenResult(string AccessToken, DateTimeOffset ExpiresAt);
