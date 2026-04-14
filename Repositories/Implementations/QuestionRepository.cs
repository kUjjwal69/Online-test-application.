using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Question?> GetByIdAsync(Guid id)
            => await _context.Questions.FindAsync(id);

        public async Task<IEnumerable<Question>> GetByTestIdAsync(Guid testId)
            => await _context.Questions
                .Where(q => q.TestId == testId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

        public async Task<Question> CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task UpdateAsync(Question question)
        {
            _context.Questions.Update(question);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var q = await _context.Questions.FindAsync(id)
                ?? throw new KeyNotFoundException($"Question {id} not found.");
            _context.Questions.Remove(q);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _context.Questions.AnyAsync(q => q.Id == id);
    }
}
