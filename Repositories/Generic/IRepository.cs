namespace TestManagementApplication.Repositories.Generic
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
       
        Task<T?> GetByIdAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(T entity);
        Task<bool> ExistsAsync(Guid id);
    }
}
