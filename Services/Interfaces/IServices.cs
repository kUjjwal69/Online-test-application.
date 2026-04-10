using TestManagementApplication.Models.DTOs.Auth;
using TestManagementApplication.Models.DTOs.Admin;
using TestManagementApplication.Models.DTOs.Candidate;
using TestManagementApplication.Models.DTOs.Proctoring;

namespace TestManagementApplication.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

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

        // Sessions
        Task<IEnumerable<SessionSummaryResponse>> GetAllSessionsAsync();
        Task<SessionSummaryResponse> GetSessionByIdAsync(Guid sessionId);
        Task SuspendSessionAsync(Guid sessionId);
        Task<IEnumerable<ViolationResponse>> GetSessionViolationsAsync(Guid sessionId);
        Task<IEnumerable<ScreenshotResponse>> GetSessionScreenshotsAsync(Guid sessionId);
        Task<IEnumerable<VideoRecordingResponse>> GetSessionVideosAsync(Guid sessionId);

        // Users
        Task<IEnumerable<UserResponse>> GetAllCandidatesAsync();
    }

    public interface ICandidateService
    {
        Task<IEnumerable<AssignedTestResponse>> GetAssignedTestsAsync(Guid userId);
        Task<StartTestResponse> StartTestAsync(Guid testId, Guid userId);
        Task<IEnumerable<QuestionForCandidateResponse>> GetQuestionsAsync(Guid sessionId, Guid userId);
        Task<SubmitAnswerResponse> SubmitAnswerAsync(Guid sessionId, Guid userId, SubmitAnswerRequest request);
        Task<TestResultResponse> SubmitTestAsync(Guid sessionId, Guid userId);
        Task<TestResultResponse> GetResultAsync(Guid sessionId, Guid userId);
    }

    public interface IProctoringService
    {
        Task<UploadScreenshotResponse> UploadScreenshotAsync(Guid sessionId, Guid userId, UploadScreenshotRequest request);
        Task<ReportViolationResponse> ReportViolationAsync(Guid sessionId, Guid userId, ReportViolationRequest request);
        Task<VideoChunkUploadResponse> UploadVideoChunkAsync(Guid sessionId, Guid userId, IFormFile chunk, string videoType, int chunkIndex, Guid? recordingId);
        Task<VideoCompleteResponse> CompleteVideoAsync(Guid sessionId, Guid userId, VideoCompleteRequest request);
        Task<SessionHeartbeatResponse> HeartbeatAsync(Guid sessionId, Guid userId, string? ip, string? userAgent, SessionHeartbeatRequest request);
    }
}
