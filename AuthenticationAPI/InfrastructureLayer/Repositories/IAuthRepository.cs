using AuthenticationAPI.ApplicationLayer.DTOs;
using AuthenticationAPI.Models;
namespace AuthenticationAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<Users?> GetActiveUserAsync(LoginDTO dto);
        Task<Users?> GetUserAsync(int userid);


    }
}
