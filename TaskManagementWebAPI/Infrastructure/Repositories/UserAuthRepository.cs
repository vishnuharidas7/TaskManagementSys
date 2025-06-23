using LoggingLibrary.Interfaces;
using MathNet.Numerics.RootFinding;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tls;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;

namespace TaskManagementWebAPI.Infrastructure.Repositories
{
    public class UserAuthRepository : IUserAuthRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IAppLogger<UserAuthRepository> _logger;

        public UserAuthRepository(HttpClient httpClient, IAppLogger<UserAuthRepository> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> LoginAsync(LoginDTO dto)
        {
            try
            {
                var requestUrl = "https://localhost:7268/api/Auth/login";
                var response = await _httpClient.PostAsJsonAsync(requestUrl, dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Login failed: {error}");
            }
            catch (Exception ex)
            {
                _logger.LoggWarning("LoginAsync-Login failed");
                throw;
            }
        }

        public async Task<string> Refresh(TokenResponseDTO dto)
        {
            try
            {
                var requestUrl = "https://localhost:7268/api/Auth/refresh";
                var response = await _httpClient.PostAsJsonAsync(requestUrl, dto);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Token refresh failed: {error}");
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
