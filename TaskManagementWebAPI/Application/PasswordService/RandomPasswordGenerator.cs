using LoggingLibrary.Interfaces;
using System.Net.Http;
using TaskManagementWebAPI.Application.Interfaces;

namespace TaskManagementWebAPI.Application.PasswordService
{
    public class RandomPasswordGenerator: IRandomPasswordGenerator
    {
        private IAppLogger<RandomPasswordGenerator> _logger;
        public RandomPasswordGenerator(IAppLogger<RandomPasswordGenerator> logger)
        {
            _logger=logger ?? throw new ArgumentNullException(nameof(logger));
        }
        private const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        public string GenerateRandomPassword(int length)
        {
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
