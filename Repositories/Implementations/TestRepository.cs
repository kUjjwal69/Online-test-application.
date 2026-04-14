using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class TestRepository : Repository<Test>, ITestRepository
    {
        public TestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Test?> GetByIdAsync(Guid id)
            => await _context.Tests.Include(t => t.Creator).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Test?> GetByIdWithQuestionsAsync(Guid id)
            => await _context.Tests
                .Include(t => t.Creator)
                .Include(t => t.Questions.OrderBy(q => q.OrderIndex))
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Test>> GetAllAsync()
            => await _context.Tests
                .Include(t => t.Creator)
                .Include(t => t.Questions)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<Test> CreateAsync(Test test)
        {
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();
            return test;
        }

        public async Task UpdateAsync(Test test)
        {
            _context.Tests.Update(test);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var test = await _context.Tests.FindAsync(id)
                ?? throw new KeyNotFoundException($"Test {id} not found.");
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _context.Tests.AnyAsync(t => t.Id == id);
    }
}
