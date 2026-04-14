using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class TestSessionRepository : Repository<TestSession>, ITestSessionRepository
    {
        public TestSessionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<TestSession?> GetByIdAsync(Guid id)
            => await _context.TestSessions
                .Include(s => s.Test)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<TestSession?> GetByIdWithDetailsAsync(Guid id)
            => await _context.TestSessions
                .Include(s => s.Test)
                .Include(s => s.User)
                .Include(s => s.UserAnswers)
                .Include(s => s.Violations)
                .Include(s => s.CapturedImages)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<TestSession?> GetActiveSessionAsync(Guid userId, Guid testId)
            => await _context.TestSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.TestId == testId && s.Status == "Active");

        public async Task<IEnumerable<TestSession>> GetAllAsync()
            => await _context.TestSessions
                .Include(s => s.Test)
                .Include(s => s.User)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

        public async Task<IEnumerable<TestSession>> GetByUserIdAsync(Guid userId)
            => await _context.TestSessions
                .Include(s => s.Test)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

        public async Task<TestSession> CreateAsync(TestSession session)
        {
            _context.TestSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task UpdateAsync(TestSession session)
        {   
            _context.TestSessions.Update(session);
            await _context.SaveChangesAsync();
        }
    }

}
