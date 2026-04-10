using TestManagementApplication.Models.Entities;

namespace TestManagementApplication.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllCandidatesAsync();
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> ExistsAsync(Guid id);
    }

    public interface ITestRepository
    {
        Task<Test?> GetByIdAsync(Guid id);
        Task<Test?> GetByIdWithQuestionsAsync(Guid id);
        Task<IEnumerable<Test>> GetAllAsync();
        Task<Test> CreateAsync(Test test);
        Task UpdateAsync(Test test);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }

    public interface IQuestionRepository
    {
        Task<Question?> GetByIdAsync(Guid id);
        Task<IEnumerable<Question>> GetByTestIdAsync(Guid testId);
        Task<Question> CreateAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }

    public interface ITestAssignmentRepository
    {
        Task<TestAssignment?> GetAsync(Guid testId, Guid userId);
        Task<IEnumerable<TestAssignment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<TestAssignment>> GetByTestIdAsync(Guid testId);
        Task<TestAssignment> CreateAsync(TestAssignment assignment);
        Task<bool> ExistsAsync(Guid testId, Guid userId);
    }

    public interface ITestSessionRepository
    {
        Task<TestSession?> GetByIdAsync(Guid id);
        Task<TestSession?> GetByIdWithDetailsAsync(Guid id);
        Task<TestSession?> GetActiveSessionAsync(Guid userId, Guid testId);
        Task<IEnumerable<TestSession>> GetAllAsync();
        Task<IEnumerable<TestSession>> GetByUserIdAsync(Guid userId);
        Task<TestSession> CreateAsync(TestSession session);
        Task UpdateAsync(TestSession session);
    }

    public interface IUserAnswerRepository
    {
        Task<UserAnswer?> GetBySessionAndQuestionAsync(Guid sessionId, Guid questionId);
        Task<IEnumerable<UserAnswer>> GetBySessionIdAsync(Guid sessionId);
        Task<UserAnswer> CreateAsync(UserAnswer answer);
        Task UpdateAsync(UserAnswer answer);
        Task<int> CountCorrectAsync(Guid sessionId);
    }

    public interface IViolationRepository
    {
        Task<IEnumerable<Violation>> GetBySessionIdAsync(Guid sessionId);
        Task<Violation> CreateAsync(Violation violation);
        Task<int> CountBySessionIdAsync(Guid sessionId);
    }

    public interface ICapturedImageRepository
    {
        Task<IEnumerable<CapturedImage>> GetBySessionIdAsync(Guid sessionId);
        Task<CapturedImage> CreateAsync(CapturedImage image);
    }

    public interface IVideoRecordingRepository
    {
        Task<VideoRecording?> GetByIdAsync(Guid id);
        Task<IEnumerable<VideoRecording>> GetBySessionIdAsync(Guid sessionId);
        Task<VideoRecording> CreateAsync(VideoRecording recording);
        Task UpdateAsync(VideoRecording recording);
    }
}
