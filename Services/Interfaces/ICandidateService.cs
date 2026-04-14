using TestManagementApplication.Models.DTOs.Candidate;

namespace TestManagementApplication.Services.Interfaces
{
    public interface ICandidateService
    {
        Task<IEnumerable<AssignedTestResponse>> GetAssignedTestsAsync(Guid userId);
        Task<StartTestResponse> StartTestAsync(Guid testId, Guid userId);
        Task<IEnumerable<QuestionForCandidateResponse>> GetQuestionsAsync(Guid sessionId, Guid userId);
        Task<SubmitAnswerResponse> SubmitAnswerAsync(Guid sessionId, Guid userId, SubmitAnswerRequest request);
        Task<TestResultResponse> SubmitTestAsync(Guid sessionId, Guid userId);
        Task<TestResultResponse> GetResultAsync(Guid sessionId, Guid userId);
    }
}
