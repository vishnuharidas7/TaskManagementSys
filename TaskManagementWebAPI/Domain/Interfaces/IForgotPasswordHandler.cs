using Microsoft.AspNetCore.Identity.Data;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IForgotPasswordHandler
    {

        /// <summary>
        /// For Forgot password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Users?> HandleAsync(ForgotPasswordRequest request);
    }
}
