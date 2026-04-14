namespace TestManagementApplication.Services.Implementations
{
    using System.Security.Cryptography;
    using TestManagementApplication.Models.Entities;
    using TestManagementApplication.Services.Interfaces;

    public class TokenService : ITokenService
    {
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
