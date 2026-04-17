using Azure.Core;
using TestManagementApplication.Models.DTOs.Candidate;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Interfaces;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Services.Implementations
{
    public class CandidateService : ICandidateService
    {
        private readonly ITestAssignmentRepository _assignmentRepo;
        private readonly ITestSessionRepository _sessionRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly IUserAnswerRepository _answerRepo;
        private readonly ITestRepository _testRepo;

        public CandidateService(
            ITestAssignmentRepository assignmentRepo,
            ITestSessionRepository sessionRepo,
            IQuestionRepository questionRepo,
            IUserAnswerRepository answerRepo,
            ITestRepository testRepo)
        {
            _assignmentRepo = assignmentRepo;
            _sessionRepo = sessionRepo;
            _questionRepo = questionRepo;
            _answerRepo = answerRepo;
            _testRepo = testRepo;
        }

        public async Task<IEnumerable<AssignedTestResponse>> GetAssignedTestsAsync(Guid userId)
        {
            var assignments = await _assignmentRepo.GetByUserIdAsync(userId);
            var result = new List<AssignedTestResponse>();

            foreach (var a in assignments)
            {
                if (a.Test == null || !a.Test.IsActive) continue;

                // Check if there's a completed/suspended session
                var sessions = await _sessionRepo.GetByUserIdAsync(userId);
                var alreadyAttempted = sessions.Any(s =>
                    s.TestId == a.TestId &&
                    (s.Status == SessionStatus.Completed || s.Status == SessionStatus.Suspended));

                result.Add(new AssignedTestResponse
                {
                    TestId = a.TestId,
                    Title = a.Test.Title,
                    Description = a.Test.Description,
                    DurationMinutes = a.Test.DurationMinutes,
                    TotalMarks = a.Test.TotalMarks,
                    PassingMarks = a.Test.PassingMarks,
                    AssignedAt = a.AssignedAt,
                    ExpiresAt = a.ExpiresAt,
                    AlreadyAttempted = alreadyAttempted,
                    // ✅ dynamic + default fallback
                    Status = string.IsNullOrEmpty(a.Status)
                        ? AssignmentStatus.NotStarted
                        : a.Status
                });
            }

            return result;
        }

        public async Task<StartTestResponse> StartTestAsync(Guid testId, Guid userId)
        {
            // Validate assignment
            var assignment = await _assignmentRepo.GetAsync(testId, userId);
            if (assignment == null)
                throw new InvalidOperationException("You are not assigned to this test.");
            if (assignment.ExpiresAt.HasValue && assignment.ExpiresAt.Value <= DateTime.UtcNow)
                throw new InvalidOperationException("This test assignment has expired.");

            // Check for active session
            var existingActive = await _sessionRepo.GetActiveSessionAsync(userId, testId);
            if (existingActive != null)
            {
                // Resume existing active session
                var existTest = await _testRepo.GetByIdAsync(testId);
                return new StartTestResponse
                {
                    SessionId = existingActive.Id,
                    TestId = testId,
                    TestTitle = existTest?.Title ?? "",
                    DurationMinutes = existTest?.DurationMinutes ?? 0,
                    StartTime = existingActive.StartTime,
                    ViolationThreshold = existTest?.ViolationThreshold ?? 3
                };
            }

            // Check if already completed
            var userSessions = await _sessionRepo.GetByUserIdAsync(userId);
            if (userSessions.Any(s => s.TestId == testId &&
                (s.Status == SessionStatus.Completed || s.Status == SessionStatus.Suspended)))
                throw new InvalidOperationException("You have already completed or been suspended from this test.");

            var test = await _testRepo.GetByIdAsync(testId)
                ?? throw new KeyNotFoundException($"Test {testId} not found.");

            if (!test.IsActive)
                throw new InvalidOperationException("This test is no longer active.");

            var session = new TestSession
            {
                TestId = testId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                Status = SessionStatus.Active,
                ViolationCount = 0
            };

            await _sessionRepo.AddAsync(session);

            return new StartTestResponse
            {
                SessionId = session.Id,
                TestId = testId,
                TestTitle = test.Title,
                DurationMinutes = test.DurationMinutes,
                StartTime = session.StartTime,
                ViolationThreshold = test.ViolationThreshold
            };
        }

        public async Task<IEnumerable<QuestionForCandidateResponse>> GetQuestionsAsync(Guid sessionId, Guid userId)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);
            var questions = await _questionRepo.GetByTestIdAsync(session.TestId);

            return questions.Select(q => new QuestionForCandidateResponse
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                Marks = q.Marks,
                OrderIndex = q.OrderIndex
            });
        }

        public async Task<SubmitAnswerResponse> SubmitAnswerAsync(Guid sessionId, Guid userId, SubmitAnswerRequest request)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);

            var validOptions = new[] { "A", "B", "C", "D" };
            if (!validOptions.Contains(request.SelectedOption.ToUpper()))
                throw new ArgumentException("SelectedOption must be A, B, C, or D.");

            var question = await _questionRepo.GetByIdAsync(request.QuestionId)
                ?? throw new KeyNotFoundException($"Question {request.QuestionId} not found.");

            if (question.TestId != session.TestId)
                throw new InvalidOperationException("This question does not belong to the current test.");

            // Upsert: update if already answered
            var existing = await _answerRepo.GetBySessionAndQuestionAsync(sessionId, request.QuestionId);
            if (existing != null)
            {
                existing.SelectedOption = request.SelectedOption.ToUpper();
                existing.IsCorrect = string.Equals(existing.SelectedOption, question.CorrectOption, StringComparison.OrdinalIgnoreCase);
                existing.SubmittedAt = DateTime.UtcNow;
                await _answerRepo.UpdateAsync(existing);
                return new SubmitAnswerResponse
                {
                    AnswerId = existing.Id,
                    QuestionId = existing.QuestionId,
                    SelectedOption = existing.SelectedOption,
                    SubmittedAt = existing.SubmittedAt
                };
            }

            var answer = new UserAnswer
            {
                SessionId = sessionId,
                QuestionId = request.QuestionId,
                SelectedOption = request.SelectedOption.ToUpper(),
                IsCorrect = string.Equals(request.SelectedOption.ToUpper(), question.CorrectOption, StringComparison.OrdinalIgnoreCase),
                SubmittedAt = DateTime.UtcNow
            };

            await _answerRepo.AddAsync(answer);
            return new SubmitAnswerResponse
            {
                AnswerId = answer.Id,
                QuestionId = answer.QuestionId,
                SelectedOption = answer.SelectedOption,
                SubmittedAt = answer.SubmittedAt
            };
        }

        public async Task<TestResultResponse> SubmitTestAsync(Guid sessionId, Guid userId)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);
            return await FinalizeSessionAsync(session);
        }

        public async Task<TestResultResponse> GetResultAsync(Guid sessionId, Guid userId)
        {
            var session = await _sessionRepo.GetByIdWithDetailsAsync(sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            if (session.UserId != userId)
                throw new UnauthorizedAccessException("You do not have access to this session.");

            if (session.Status == SessionStatus.Active)
                throw new InvalidOperationException("Test has not been submitted yet.");

            var test = session.Test!;
            var answers = await _answerRepo.GetBySessionIdAsync(sessionId);
            int correctCount = answers.Count(a => a.IsCorrect);
            int totalQuestions = (await _questionRepo.GetByTestIdAsync(session.TestId)).Count();

            return new TestResultResponse
            {
                SessionId = session.Id,
                TestTitle = test.Title,
                TotalMarks = test.TotalMarks,
                PassingMarks = test.PassingMarks,
                Score = session.Score ?? 0,
                Percentage = session.Percentage ?? 0,
                IsPassed = session.IsPassed ?? false,
                Status = session.Status,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                ViolationCount = session.ViolationCount,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctCount
            };
        }

        // ─── PRIVATE HELPERS ──────────────────────────────────────────────

        private async Task<TestSession> ValidateActiveSessionAsync(Guid sessionId, Guid userId)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            if (session.UserId != userId)
                throw new UnauthorizedAccessException("You do not have access to this session.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException($"Session is {session.Status}. No further actions allowed.");

            return session;
        }

        private async Task<TestResultResponse> FinalizeSessionAsync(TestSession session)
        {
            var test = await _testRepo.GetByIdWithQuestionsAsync(session.TestId)!
                ?? throw new KeyNotFoundException("Test not found.");

            var answers = await _answerRepo.GetBySessionIdAsync(session.Id);

            // Calculate score based on marks per question
            int score = 0;
            foreach (var answer in answers.Where(a => a.IsCorrect))
            {
                var question = test.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question != null) score += question.Marks;
            }

            decimal percentage = test.TotalMarks > 0
                ? Math.Round((decimal)score / test.TotalMarks * 100, 2)
                : 0;

            session.Score = score;
            session.Percentage = percentage;
            session.IsPassed = score >= test.PassingMarks;
            session.Status = SessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;

            await _sessionRepo.UpdateAsync(session);

            // Update assignment status to Completed
            var assignment = await _assignmentRepo.GetAsync(session.TestId, session.UserId);
            if (assignment != null)
            {
                assignment.Status = AssignmentStatus.Completed;
                await _assignmentRepo.UpdateAsync(assignment);
            }

            int correctCount = answers.Count(a => a.IsCorrect);

            return new TestResultResponse
            {
                SessionId = session.Id,
                TestTitle = test.Title,
                TotalMarks = test.TotalMarks,
                PassingMarks = test.PassingMarks,
                Score = score,
                Percentage = percentage,
                IsPassed = session.IsPassed ?? false,
                Status = session.Status,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                ViolationCount = session.ViolationCount,
                TotalQuestions = test.Questions.Count,
                CorrectAnswers = correctCount
            };
        }
    }
}
