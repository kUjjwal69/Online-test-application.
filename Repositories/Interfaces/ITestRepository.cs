using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface ITestRepository : IRepository<Test>
    {
        Task<Test?> GetByIdWithQuestionsAsync(Guid id);
    }
}
