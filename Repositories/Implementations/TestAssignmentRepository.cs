using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class TestAssignmentRepository : Repository<TestAssignment>, ITestAssignmentRepository
    {
        public TestAssignmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<TestAssignment?> GetAsync(Guid testId, Guid userId)
            => await _context.TestAssignments
                .FirstOrDefaultAsync(a => a.TestId == testId && a.UserId == userId);

        public async Task<IEnumerable<TestAssignment>> GetByUserIdAsync(Guid userId)
            => await _context.TestAssignments
                .Include(a => a.Test)
                .Where(a => a.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<TestAssignment>> GetByTestIdAsync(Guid testId)
            => await _context.TestAssignments
                .Include(a => a.User)
                .Where(a => a.TestId == testId)
                .ToListAsync();

        public async Task<TestAssignment> CreateAsync(TestAssignment assignment)
        {
            _context.TestAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        

        public async Task<bool> ExistsAsync(Guid testId, Guid userId)
            => await _context.TestAssignments.AnyAsync(a => a.TestId == testId && a.UserId == userId);

        public async Task<TestAssignment> DeleteAsync(Guid testId, Guid userId)
        {
            var assignment = await _context.TestAssignments
                .FirstOrDefaultAsync(a => a.TestId == testId && a.UserId == userId);

            if (assignment == null)
                throw new KeyNotFoundException("Assignment not found.");

            _context.TestAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return assignment;
        }
    }
}
