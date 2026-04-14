using TestManagementApplication.Models.DTOs.Admin;

namespace TestManagementApplication.Services.Interfaces
{
    public interface IAdminService
    {
        // Tests
        Task<TestResponse> CreateTestAsync(CreateTestRequest request, Guid adminId);
        Task<TestResponse> UpdateTestAsync(Guid testId, UpdateTestRequest request);
        Task DeleteTestAsync(Guid testId);
        Task<IEnumerable<TestResponse>> GetAllTestsAsync();

        // Questions
        Task<QuestionResponse> AddQuestionAsync(Guid testId, CreateQuestionRequest request);
        Task<QuestionResponse> UpdateQuestionAsync(Guid questionId, UpdateQuestionRequest request);
        Task DeleteQuestionAsync(Guid questionId);
        Task<IEnumerable<QuestionResponse>> GetQuestionsByTestAsync(Guid testId);

        // Assignments
        Task AssignTestAsync(Guid testId, AssignTestRequest request);
        Task UnassignTestAsync(Guid testId, Guid userId);

        // Sessions
        Task<IEnumerable<SessionSummaryResponse>> GetAllSessionsAsync();
        Task<SessionSummaryResponse> GetSessionByIdAsync(Guid sessionId);
        Task SuspendSessionAsync(Guid sessionId);
        Task<IEnumerable<ViolationResponse>> GetSessionViolationsAsync(Guid sessionId);
        Task<IEnumerable<ScreenshotResponse>> GetSessionScreenshotsAsync(Guid sessionId);
        Task<IEnumerable<VideoRecordingResponse>> GetSessionVideosAsync(Guid sessionId);

        // Users
        Task<IEnumerable<UserResponse>> GetAllCandidatesAsync();
        Task<UserResponse> GetCandidatesByIdAsync(Guid id);
    }
}
