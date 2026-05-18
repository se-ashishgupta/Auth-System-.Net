using AuthSystem.Models;

namespace AuthSystem.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}