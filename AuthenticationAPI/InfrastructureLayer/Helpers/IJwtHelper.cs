using AuthenticationAPI.Models;

namespace AuthenticationAPI.InfrastructureLayer.Helpers
{
    public interface IJwtHelper
    {
        /// <summary>
        /// For Generate Access Token
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        string GenerateAccessToken(Users User);
        /// <summary>
        /// For Generate Refresh Token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string GenerateRefreshToken(Users user);
    }
}
