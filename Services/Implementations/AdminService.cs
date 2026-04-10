using TestManagementApplication.Models.DTOs.Admin;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Interfaces;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ITestRepository _testRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly ITestAssignmentRepository _assignmentRepo;
        private readonly ITestSessionRepository _sessionRepo;
        private readonly IViolationRepository _violationRepo;
        private readonly ICapturedImageRepository _imageRepo;
        private readonly IVideoRecordingRepository _videoRepo;
        private readonly IUserRepository _userRepo;

        public AdminService(
            ITestRepository testRepo,
            IQuestionRepository questionRepo,
            ITestAssignmentRepository assignmentRepo,
            ITestSessionRepository sessionRepo,
            IViolationRepository violationRepo,
            ICapturedImageRepository imageRepo,
            IVideoRecordingRepository videoRepo,
            IUserRepository userRepo)
        {
            _testRepo = testRepo;
            _questionRepo = questionRepo;
            _assignmentRepo = assignmentRepo;
            _sessionRepo = sessionRepo;
            _violationRepo = violationRepo;
            _imageRepo = imageRepo;
            _videoRepo = videoRepo;
            _userRepo = userRepo;
        }

        // ─── TESTS ────────────────────────────────────────────────────────

        public async Task<TestResponse> CreateTestAsync(CreateTestRequest request, Guid adminId)
        {
            ValidateTestRequest(request);

            var test = new Test
            {
                Title = request.Title,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                TotalMarks = request.TotalMarks,
                PassingMarks = request.PassingMarks,
                ViolationThreshold = request.ViolationThreshold,
                IsActive = true,
                CreatedBy = adminId
            };

            await _testRepo.CreateAsync(test);
            var creator = await _userRepo.GetByIdAsync(adminId);
            return MapTestResponse(test, creator, 0);
        }

        public async Task<TestResponse> UpdateTestAsync(Guid testId, UpdateTestRequest request)
        {
            var test = await _testRepo.GetByIdWithQuestionsAsync(testId)
                ?? throw new KeyNotFoundException($"Test with ID {testId} not found.");

            test.Title = request.Title;
            test.Description = request.Description;
            test.DurationMinutes = request.DurationMinutes;
            test.TotalMarks = request.TotalMarks;
            test.PassingMarks = request.PassingMarks;
            test.ViolationThreshold = request.ViolationThreshold;
            test.IsActive = request.IsActive;

            await _testRepo.UpdateAsync(test);
            var creator = await _userRepo.GetByIdAsync(test.CreatedBy);
            return MapTestResponse(test, creator, test.Questions.Count);
        }

        public async Task DeleteTestAsync(Guid testId)
        {
            if (!await _testRepo.ExistsAsync(testId))
                throw new KeyNotFoundException($"Test with ID {testId} not found.");
            await _testRepo.DeleteAsync(testId);
        }

        public async Task<IEnumerable<TestResponse>> GetAllTestsAsync()
        {
            var tests = await _testRepo.GetAllAsync();
            return tests.Select(t => MapTestResponse(t, t.Creator, t.Questions.Count));
        }

        // ─── QUESTIONS ────────────────────────────────────────────────────

        public async Task<QuestionResponse> AddQuestionAsync(Guid testId, CreateQuestionRequest request)
        {
            if (!await _testRepo.ExistsAsync(testId))
                throw new KeyNotFoundException($"Test with ID {testId} not found.");

            ValidateQuestionRequest(request.CorrectOption);

            var question = new Question
            {
                TestId = testId,
                QuestionText = request.QuestionText,
                OptionA = request.OptionA,
                OptionB = request.OptionB,
                OptionC = request.OptionC,
                OptionD = request.OptionD,
                CorrectOption = request.CorrectOption.ToUpper(),
                Marks = request.Marks,
                OrderIndex = request.OrderIndex
            };

            await _questionRepo.CreateAsync(question);
            return MapQuestionResponse(question);
        }

        public async Task<QuestionResponse> UpdateQuestionAsync(Guid questionId, UpdateQuestionRequest request)
        {
            var question = await _questionRepo.GetByIdAsync(questionId)
                ?? throw new KeyNotFoundException($"Question with ID {questionId} not found.");

            ValidateQuestionRequest(request.CorrectOption);

            question.QuestionText = request.QuestionText;
            question.OptionA = request.OptionA;
            question.OptionB = request.OptionB;
            question.OptionC = request.OptionC;
            question.OptionD = request.OptionD;
            question.CorrectOption = request.CorrectOption.ToUpper();
            question.Marks = request.Marks;
            question.OrderIndex = request.OrderIndex;

            await _questionRepo.UpdateAsync(question);
            return MapQuestionResponse(question);
        }

        public async Task DeleteQuestionAsync(Guid questionId)
        {
            if (!await _questionRepo.ExistsAsync(questionId))
                throw new KeyNotFoundException($"Question with ID {questionId} not found.");
            await _questionRepo.DeleteAsync(questionId);
        }

        public async Task<IEnumerable<QuestionResponse>> GetQuestionsByTestAsync(Guid testId)
        {
            if (!await _testRepo.ExistsAsync(testId))
                throw new KeyNotFoundException($"Test with ID {testId} not found.");
            var questions = await _questionRepo.GetByTestIdAsync(testId);
            return questions.Select(MapQuestionResponse);
        }

        // ─── ASSIGNMENTS ──────────────────────────────────────────────────

        public async Task AssignTestAsync(Guid testId, AssignTestRequest request)
        {
            if (!await _testRepo.ExistsAsync(testId))
                throw new KeyNotFoundException($"Test with ID {testId} not found.");
            if (!await _userRepo.ExistsAsync(request.UserId))
                throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
            if (await _assignmentRepo.ExistsAsync(testId, request.UserId))
                throw new InvalidOperationException("Test is already assigned to this user.");

            var assignment = new TestAssignment
            {
                TestId = testId,
                UserId = request.UserId,
                ExpiresAt = request.ExpiresAt
            };
            await _assignmentRepo.CreateAsync(assignment);
        }

        // ─── SESSIONS ─────────────────────────────────────────────────────

        public async Task<IEnumerable<SessionSummaryResponse>> GetAllSessionsAsync()
        {
            var sessions = await _sessionRepo.GetAllAsync();
            return sessions.Select(MapSessionSummary);
        }

        public async Task<SessionSummaryResponse> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId)
                ?? throw new KeyNotFoundException($"Session with ID {sessionId} not found.");
            return MapSessionSummary(session);
        }

        public async Task SuspendSessionAsync(Guid sessionId)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId)
                ?? throw new KeyNotFoundException($"Session with ID {sessionId} not found.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException($"Session is already {session.Status}.");

            session.Status = SessionStatus.Suspended;
            session.EndTime = DateTime.UtcNow;
            await _sessionRepo.UpdateAsync(session);
        }

        public async Task<IEnumerable<ViolationResponse>> GetSessionViolationsAsync(Guid sessionId)
        {
            if (await _sessionRepo.GetByIdAsync(sessionId) == null)
                throw new KeyNotFoundException($"Session with ID {sessionId} not found.");

            var violations = await _violationRepo.GetBySessionIdAsync(sessionId);
            return violations.Select(v => new ViolationResponse
            {
                Id = v.Id,
                ViolationType = v.ViolationType,
                OccurredAt = v.OccurredAt,
                Details = v.Details
            });
        }

        public async Task<IEnumerable<ScreenshotResponse>> GetSessionScreenshotsAsync(Guid sessionId)
        {
            if (await _sessionRepo.GetByIdAsync(sessionId) == null)
                throw new KeyNotFoundException($"Session with ID {sessionId} not found.");

            var images = await _imageRepo.GetBySessionIdAsync(sessionId);
            return images.Select(i => new ScreenshotResponse
            {
                Id = i.Id,
                FilePath = i.FilePath,
                CapturedAt = i.CapturedAt
            });
        }

        public async Task<IEnumerable<VideoRecordingResponse>> GetSessionVideosAsync(Guid sessionId)
        {
            if (await _sessionRepo.GetByIdAsync(sessionId) == null)
                throw new KeyNotFoundException($"Session with ID {sessionId} not found.");

            var videos = await _videoRepo.GetBySessionIdAsync(sessionId);
            return videos.Select(v => new VideoRecordingResponse
            {
                Id = v.Id,
                FilePath = v.FilePath,
                Type = v.Type,
                StartTime = v.StartTime,
                EndTime = v.EndTime,
                IsComplete = v.IsComplete
            });
        }

        // ─── USERS ────────────────────────────────────────────────────────

        public async Task<IEnumerable<UserResponse>> GetAllCandidatesAsync()
        {
            var users = await _userRepo.GetAllCandidatesAsync();
            return users.Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
        }

        // ─── PRIVATE MAPPERS ──────────────────────────────────────────────

        private static TestResponse MapTestResponse(Test test, User? creator, int questionCount) => new()
        {
            Id = test.Id,
            Title = test.Title,
            Description = test.Description,
            DurationMinutes = test.DurationMinutes,
            TotalMarks = test.TotalMarks,
            PassingMarks = test.PassingMarks,
            ViolationThreshold = test.ViolationThreshold,
            IsActive = test.IsActive,
            CreatedAt = test.CreatedAt,
            CreatedByUsername = creator?.Username ?? "Unknown",
            QuestionCount = questionCount
        };

        private static QuestionResponse MapQuestionResponse(Question q) => new()
        {
            Id = q.Id,
            TestId = q.TestId,
            QuestionText = q.QuestionText,
            OptionA = q.OptionA,
            OptionB = q.OptionB,
            OptionC = q.OptionC,
            OptionD = q.OptionD,
            CorrectOption = q.CorrectOption,
            Marks = q.Marks,
            OrderIndex = q.OrderIndex
        };

        private static SessionSummaryResponse MapSessionSummary(TestSession s) => new()
        {
            Id = s.Id,
            CandidateUsername = s.User?.Username ?? "Unknown",
            TestTitle = s.Test?.Title ?? "Unknown",
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Status = s.Status,
            ViolationCount = s.ViolationCount,
            Score = s.Score,
            Percentage = s.Percentage,
            IsPassed = s.IsPassed
        };

        private static void ValidateTestRequest(CreateTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title is required.");
            if (request.DurationMinutes <= 0)
                throw new ArgumentException("Duration must be greater than 0.");
            if (request.TotalMarks <= 0)
                throw new ArgumentException("Total marks must be greater than 0.");
            if (request.PassingMarks < 0 || request.PassingMarks > request.TotalMarks)
                throw new ArgumentException("Passing marks must be between 0 and total marks.");
        }

        private static void ValidateQuestionRequest(string correctOption)
        {
            var valid = new[] { "A", "B", "C", "D" };
            if (!valid.Contains(correctOption.ToUpper()))
                throw new ArgumentException("CorrectOption must be A, B, C, or D.");
        }
    }
}
