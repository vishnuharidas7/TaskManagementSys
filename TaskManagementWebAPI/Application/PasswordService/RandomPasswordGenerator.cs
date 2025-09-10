using LoggingLibrary.Interfaces;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;

namespace TaskManagementWebAPI.Application.PasswordService
{
    public class RandomPasswordGenerator: IRandomPasswordGenerator
    {
        private readonly IAppLogger<RandomPasswordGenerator> _logger;
        public RandomPasswordGenerator(IAppLogger<RandomPasswordGenerator> logger)
        {
            _logger=logger ?? throw new ArgumentNullException(nameof(logger));
        }
        private const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        public string GenerateRandomPassword(int length)
        {
            if (length <= 0)
            {
                _logger.LoggError(new ArgumentOutOfRangeException(nameof(length)), "Invalid password length requested: {Length}", length);
                throw new ArgumentOutOfRangeException(nameof(length), "Password length must be greater than zero.");
            }
            try
            {
                var random = new Random();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LoggError(ex, "Invalid password length requested: {Length}", length);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LoggError(ex, "An error occurred while generating a random password.");
                throw;
            }
        }


    }
}
