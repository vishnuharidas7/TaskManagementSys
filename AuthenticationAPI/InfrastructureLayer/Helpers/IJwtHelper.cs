using AuthenticationAPI.Models;

namespace AuthenticationAPI.InfrastructureLayer.Helpers
{
    public interface IJwtHelper
    {
        string GenerateAccessToken(Users User);
        string GenerateRefreshToken(Users user);
    }
}
