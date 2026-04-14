using TestManagementApplication.Models.Entities;

namespace TestManagementApplication.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateRefreshToken();
    }
}
