using AuthenticationAPI.Models;

namespace AuthenticationAPI.Helpers
{
    public interface IJwtHelper
    {
        string GenerateAccessToken(Users User);
        string GenerateRefreshToken(Users user);
    }
}
