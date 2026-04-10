using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User?> GetByIdAsync(Guid id)
            => await _db.Users.FindAsync(id);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<User?> GetByEmailAsync(string email)
            => await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<IEnumerable<User>> GetAllCandidatesAsync()
            => await _db.Users.Where(u => u.Role == "User").ToListAsync();

        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _db.Users.AnyAsync(u => u.Id == id);
    }

    public class TestRepository : ITestRepository
    {
        private readonly AppDbContext _db;
        public TestRepository(AppDbContext db) => _db = db;

        public async Task<Test?> GetByIdAsync(Guid id)
            => await _db.Tests.Include(t => t.Creator).FirstOrDefaultAsync(t => t.Id == id);

        public async Task<Test?> GetByIdWithQuestionsAsync(Guid id)
            => await _db.Tests
                .Include(t => t.Creator)
                .Include(t => t.Questions.OrderBy(q => q.OrderIndex))
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task<IEnumerable<Test>> GetAllAsync()
            => await _db.Tests
                .Include(t => t.Creator)
                .Include(t => t.Questions)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<Test> CreateAsync(Test test)
        {
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();
            return test;
        }

        public async Task UpdateAsync(Test test)
        {
            _db.Tests.Update(test);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var test = await _db.Tests.FindAsync(id)
                ?? throw new KeyNotFoundException($"Test {id} not found.");
            _db.Tests.Remove(test);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _db.Tests.AnyAsync(t => t.Id == id);
    }

    public class QuestionRepository : IQuestionRepository
    {
        private readonly AppDbContext _db;
        public QuestionRepository(AppDbContext db) => _db = db;

        public async Task<Question?> GetByIdAsync(Guid id)
            => await _db.Questions.FindAsync(id);

        public async Task<IEnumerable<Question>> GetByTestIdAsync(Guid testId)
            => await _db.Questions
                .Where(q => q.TestId == testId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

        public async Task<Question> CreateAsync(Question question)
        {
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();
            return question;
        }

        public async Task UpdateAsync(Question question)
        {
            _db.Questions.Update(question);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var q = await _db.Questions.FindAsync(id)
                ?? throw new KeyNotFoundException($"Question {id} not found.");
            _db.Questions.Remove(q);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _db.Questions.AnyAsync(q => q.Id == id);
    }

    public class TestAssignmentRepository : ITestAssignmentRepository
    {
        private readonly AppDbContext _db;
        public TestAssignmentRepository(AppDbContext db) => _db = db;

        public async Task<TestAssignment?> GetAsync(Guid testId, Guid userId)
            => await _db.TestAssignments
                .FirstOrDefaultAsync(a => a.TestId == testId && a.UserId == userId);

        public async Task<IEnumerable<TestAssignment>> GetByUserIdAsync(Guid userId)
            => await _db.TestAssignments
                .Include(a => a.Test)
                .Where(a => a.UserId == userId)
                .ToListAsync();

        public async Task<IEnumerable<TestAssignment>> GetByTestIdAsync(Guid testId)
            => await _db.TestAssignments
                .Include(a => a.User)
                .Where(a => a.TestId == testId)
                .ToListAsync();

        public async Task<TestAssignment> CreateAsync(TestAssignment assignment)
        {
            _db.TestAssignments.Add(assignment);
            await _db.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> ExistsAsync(Guid testId, Guid userId)
            => await _db.TestAssignments.AnyAsync(a => a.TestId == testId && a.UserId == userId);
    }

    public class TestSessionRepository : ITestSessionRepository
    {
        private readonly AppDbContext _db;
        public TestSessionRepository(AppDbContext db) => _db = db;

        public async Task<TestSession?> GetByIdAsync(Guid id)
            => await _db.TestSessions
                .Include(s => s.Test)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<TestSession?> GetByIdWithDetailsAsync(Guid id)
            => await _db.TestSessions
                .Include(s => s.Test)
                .Include(s => s.User)
                .Include(s => s.UserAnswers)
                .Include(s => s.Violations)
                .Include(s => s.CapturedImages)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<TestSession?> GetActiveSessionAsync(Guid userId, Guid testId)
            => await _db.TestSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.TestId == testId && s.Status == "Active");

        public async Task<IEnumerable<TestSession>> GetAllAsync()
            => await _db.TestSessions
                .Include(s => s.Test)
                .Include(s => s.User)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

        public async Task<IEnumerable<TestSession>> GetByUserIdAsync(Guid userId)
            => await _db.TestSessions
                .Include(s => s.Test)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

        public async Task<TestSession> CreateAsync(TestSession session)
        {
            _db.TestSessions.Add(session);
            await _db.SaveChangesAsync();
            return session;
        }

        public async Task UpdateAsync(TestSession session)
        {
            _db.TestSessions.Update(session);
            await _db.SaveChangesAsync();
        }
    }

    public class UserAnswerRepository : IUserAnswerRepository
    {
        private readonly AppDbContext _db;
        public UserAnswerRepository(AppDbContext db) => _db = db;

        public async Task<UserAnswer?> GetBySessionAndQuestionAsync(Guid sessionId, Guid questionId)
            => await _db.UserAnswers
                .FirstOrDefaultAsync(a => a.SessionId == sessionId && a.QuestionId == questionId);

        public async Task<IEnumerable<UserAnswer>> GetBySessionIdAsync(Guid sessionId)
            => await _db.UserAnswers
                .Include(a => a.Question)
                .Where(a => a.SessionId == sessionId)
                .ToListAsync();

        public async Task<UserAnswer> CreateAsync(UserAnswer answer)
        {
            _db.UserAnswers.Add(answer);
            await _db.SaveChangesAsync();
            return answer;
        }

        public async Task UpdateAsync(UserAnswer answer)
        {
            _db.UserAnswers.Update(answer);
            await _db.SaveChangesAsync();
        }

        public async Task<int> CountCorrectAsync(Guid sessionId)
            => await _db.UserAnswers.CountAsync(a => a.SessionId == sessionId && a.IsCorrect);
    }

    public class ViolationRepository : IViolationRepository
    {
        private readonly AppDbContext _db;
        public ViolationRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<Violation>> GetBySessionIdAsync(Guid sessionId)
            => await _db.Violations
                .Where(v => v.SessionId == sessionId)
                .OrderByDescending(v => v.OccurredAt)
                .ToListAsync();

        public async Task<Violation> CreateAsync(Violation violation)
        {
            _db.Violations.Add(violation);
            await _db.SaveChangesAsync();
            return violation;
        }

        public async Task<int> CountBySessionIdAsync(Guid sessionId)
            => await _db.Violations.CountAsync(v => v.SessionId == sessionId);
    }

    public class CapturedImageRepository : ICapturedImageRepository
    {
        private readonly AppDbContext _db;
        public CapturedImageRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<CapturedImage>> GetBySessionIdAsync(Guid sessionId)
            => await _db.CapturedImages
                .Where(c => c.SessionId == sessionId)
                .OrderByDescending(c => c.CapturedAt)
                .ToListAsync();

        public async Task<CapturedImage> CreateAsync(CapturedImage image)
        {
            _db.CapturedImages.Add(image);
            await _db.SaveChangesAsync();
            return image;
        }
    }

    public class VideoRecordingRepository : IVideoRecordingRepository
    {
        private readonly AppDbContext _db;
        public VideoRecordingRepository(AppDbContext db) => _db = db;

        public async Task<VideoRecording?> GetByIdAsync(Guid id)
            => await _db.VideoRecordings.FindAsync(id);

        public async Task<IEnumerable<VideoRecording>> GetBySessionIdAsync(Guid sessionId)
            => await _db.VideoRecordings
                .Where(v => v.SessionId == sessionId)
                .OrderBy(v => v.StartTime)
                .ToListAsync();

        public async Task<VideoRecording> CreateAsync(VideoRecording recording)
        {
            _db.VideoRecordings.Add(recording);
            await _db.SaveChangesAsync();
            return recording;
        }

        public async Task UpdateAsync(VideoRecording recording)
        {
            _db.VideoRecordings.Update(recording);
            await _db.SaveChangesAsync();
        }
    }
}
