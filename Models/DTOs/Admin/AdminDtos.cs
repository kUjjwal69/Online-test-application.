namespace TestManagementApplication.Models.DTOs.Admin
{
    // ─── Test DTOs ───────────────────────────────────────────────
    public class CreateTestRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public int ViolationThreshold { get; set; } = 3;
    }

    public class UpdateTestRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public int ViolationThreshold { get; set; } = 3;
        public bool IsActive { get; set; }
    }

    public class TestResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public int ViolationThreshold { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
    }

    // ─── Question DTOs ────────────────────────────────────────────
    public class CreateQuestionRequest
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty; // A, B, C, D
        public int Marks { get; set; } = 1;
        public int OrderIndex { get; set; }
    }

    public class UpdateQuestionRequest
    {
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public int Marks { get; set; } = 1;
        public int OrderIndex { get; set; }
    }

    public class QuestionResponse
    {
        public Guid Id { get; set; }
        public Guid TestId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public int Marks { get; set; }
        public int OrderIndex { get; set; }
    }

    // ─── Assignment DTOs ──────────────────────────────────────────
    public class AssignTestRequest
    {
        public Guid UserId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    // ─── Session Monitor DTOs ─────────────────────────────────────
    public class SessionSummaryResponse
    {
        public Guid Id { get; set; }
        public string CandidateUsername { get; set; } = string.Empty;
        public string TestTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ViolationCount { get; set; }
        public int? Score { get; set; }
        public decimal? Percentage { get; set; }
        public bool? IsPassed { get; set; }
    }

    public class ViolationResponse
    {
        public Guid Id { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string? Details { get; set; }
    }

    public class ScreenshotResponse
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime CapturedAt { get; set; }
    }

    public class VideoRecordingResponse
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsComplete { get; set; }
    }

    // ─── User DTOs ────────────────────────────────────────────────
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
