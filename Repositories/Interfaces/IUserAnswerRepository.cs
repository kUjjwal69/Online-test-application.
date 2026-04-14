using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface IUserAnswerRepository : IRepository<UserAnswer>
    {
        Task<UserAnswer?> GetBySessionAndQuestionAsync(Guid sessionId, Guid questionId);
        Task<IEnumerable<UserAnswer>> GetBySessionIdAsync(Guid sessionId);
        Task<int> CountCorrectAsync(Guid sessionId);
    }
}
