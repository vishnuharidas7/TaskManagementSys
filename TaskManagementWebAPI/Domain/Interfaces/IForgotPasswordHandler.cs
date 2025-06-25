using Microsoft.AspNetCore.Identity.Data;
using TaskManagementWebAPI.Domain.Models;

namespace TaskManagementWebAPI.Domain.Interfaces
{
    public interface IForgotPasswordHandler
    {
        Task<Users?> HandleAsync(ForgotPasswordRequest request);
    }
}
