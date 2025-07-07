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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "httpClient cannot be null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "logger cannot be null.");
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
            catch (HttpRequestException httpEx)
            {
                _logger.LoggError(httpEx, "LoginAsync - HTTP request failed");
                throw;
            }
            catch (TaskCanceledException tcEx) when (!tcEx.CancellationToken.IsCancellationRequested)
            {
                _logger.LoggError(tcEx, "LoginAsync - Request timed out");
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LoggError(invOpEx, "LoginAsync - Login operation failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "LoginAsync - Unexpected error");
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
            catch (HttpRequestException httpEx)
            {
                _logger.LoggError(httpEx, "Refresh - HTTP request failed");
                throw;
            }
            catch (TaskCanceledException tcEx) when (!tcEx.CancellationToken.IsCancellationRequested)
            {
                _logger.LoggError(tcEx, "Refresh - Request timed out");
                throw;
            }
            catch (InvalidOperationException invOpEx)
            {
                _logger.LoggError(invOpEx, "Refresh - Token refresh operation failed");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "Refresh - Unexpected error");
                throw;
            }
        }

    }
}
