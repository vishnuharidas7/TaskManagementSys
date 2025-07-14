using LoggingLibrary.Interfaces;
using MathNet.Numerics.RootFinding;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tls;
using SendGrid.Helpers.Errors.Model;
using SendGrid.Helpers.Mail;
using System.Net;
using TaskManagementWebAPI.Application.DTOs;
using TaskManagementWebAPI.Domain.Interfaces;
using static System.Net.WebRequestMethods;

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

        //public async Task<string> LoginAsync(LoginDTO dto)
        //{
        //    try
        //    {
        //        var requestUrl = "https://localhost:7268/api/Auth/login";
        //        var response = await _httpClient.PostAsJsonAsync(requestUrl, dto);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            return await response.Content.ReadAsStringAsync();
        //        }
        //        var content = await response.Content.ReadAsStringAsync();
        //        if (response.StatusCode == HttpStatusCode.Unauthorized)
        //        {
        //            throw new UnauthorizedAccessException($"login failed: {content}");
        //        }

        //        var error = await response.Content.ReadAsStringAsync();
        //        throw new Exception($"Login failed: {content}");
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        _logger.LoggError(httpEx, "LoginAsync - HTTP request failed");
        //        throw;
        //    }
        //    catch (TaskCanceledException tcEx) when (!tcEx.CancellationToken.IsCancellationRequested)
        //    {
        //        _logger.LoggError(tcEx, "LoginAsync - Request timed out");
        //        throw;
        //    }
        //    catch (InvalidOperationException invOpEx)
        //    {
        //        _logger.LoggError(invOpEx, "LoginAsync - Login operation failed");
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LoggError(ex, "LoginAsync - Unexpected error");
        //        throw;
        //    }
        //}

        public async Task<string> LoginAsync(LoginDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    throw new ArgumentNullException(nameof(dto), "Login data (dto) cannot be null.");
                }

                var requestUrl = "https://localhost:7268/api/Auth/login";
                HttpResponseMessage response;

                try
                {
                    response = await _httpClient.PostAsJsonAsync(requestUrl, dto);
                }
                catch (Exception ex)
                {
                    _logger.LoggError(ex, "LoginAsync - HTTP request failed");
                    throw new Exception("Login service is unavailable.", ex);
                }

                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return content;
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException($"Unauthorized: {content}");

                    case HttpStatusCode.BadRequest:
                        throw new BadRequestException($"Bad request: {content}");

                    case HttpStatusCode.InternalServerError:
                        throw new Exception($"Auth service internal error: {content}");

                    default:
                        throw new Exception($"Unhandled error from Auth API ({response.StatusCode}): {content}");
                }
            }
            catch (Exception ex)
            {
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
