using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface IQuestionRepository : IRepository<Question>
    {
        Task<IEnumerable<Question>> GetByTestIdAsync(Guid testId);
    }
}
