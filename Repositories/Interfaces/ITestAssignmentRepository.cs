using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface ITestAssignmentRepository : IRepository<TestAssignment>
    {
        Task<TestAssignment?> GetAsync(Guid testId, Guid userId);
        Task<IEnumerable<TestAssignment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<TestAssignment>> GetByTestIdAsync(Guid testId);
        Task<TestAssignment> DeleteAsync(Guid testId, Guid userId);
        Task<bool> ExistsAsync(Guid testId, Guid userId);
    }
}
