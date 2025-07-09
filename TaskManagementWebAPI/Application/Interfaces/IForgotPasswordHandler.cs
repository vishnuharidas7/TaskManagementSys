using Microsoft.AspNetCore.Identity.Data;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Application.Interfaces
{
    public interface IForgotPasswordHandler
    {

        /// <summary>
        /// For Forgot password request to reset user password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Users?> HandleAsync(ForgotPasswordRequest request);
    }
}
