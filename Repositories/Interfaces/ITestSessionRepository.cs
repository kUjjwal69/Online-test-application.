using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface ITestSessionRepository: IRepository<TestSession>
    {
        Task<TestSession?> GetByIdWithDetailsAsync(Guid id);
        Task<TestSession?> GetActiveSessionAsync(Guid userId, Guid testId);
        Task<IEnumerable<TestSession>> GetByUserIdAsync(Guid userId);
    }
}
