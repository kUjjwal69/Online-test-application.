using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class UserAnswerRepository : Repository<UserAnswer>, IUserAnswerRepository
    {
        public UserAnswerRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserAnswer?> GetBySessionAndQuestionAsync(Guid sessionId, Guid questionId)
            => await _context.UserAnswers
                .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.QuestionId == questionId);

        public async Task<IEnumerable<UserAnswer>> GetBySessionIdAsync(Guid sessionId)
            => await _context.UserAnswers
                .Include(a => a.Question)
                .Where(a => a.SessionId == sessionId)
                .ToListAsync();

        public async Task<UserAnswer> CreateAsync(UserAnswer answer)
        {
            _context.UserAnswers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task UpdateAsync(UserAnswer answer)
        {
            _context.UserAnswers.Update(answer);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountCorrectAsync(Guid sessionId)
            => await _context.UserAnswers.CountAsync(a => a.SessionId == sessionId && a.IsCorrect);
    }

}
