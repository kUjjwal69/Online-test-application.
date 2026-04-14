using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface ICapturedImageRepository : IRepository<CapturedImage>
    {
        Task<IEnumerable<CapturedImage>> GetBySessionIdAsync(Guid sessionId);
    }
}
