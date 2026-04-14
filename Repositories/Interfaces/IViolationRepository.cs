using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface IViolationRepository : IRepository<Violation>
    {
        Task<IEnumerable<Violation>> GetBySessionIdAsync(Guid sessionId);
        Task<int> CountBySessionIdAsync(Guid sessionId);
    }
}
