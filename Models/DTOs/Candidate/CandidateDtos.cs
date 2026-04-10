namespace TestManagementApplication.Models.DTOs.Candidate
{
    public class AssignedTestResponse
    {
        public Guid TestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool AlreadyAttempted { get; set; }
    }

    public class StartTestResponse
    {
        public Guid SessionId { get; set; }
        public Guid TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public int ViolationThreshold { get; set; }
    }

    public class QuestionForCandidateResponse
    {
        public Guid Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public int Marks { get; set; }
        public int OrderIndex { get; set; }
        // NOTE: CorrectOption is intentionally NOT exposed here
    }

    public class SubmitAnswerRequest
    {
        public Guid QuestionId { get; set; }
        public string SelectedOption { get; set; } = string.Empty; // A, B, C, D
    }

    public class SubmitAnswerResponse
    {
        public Guid AnswerId { get; set; }
        public Guid QuestionId { get; set; }
        public string SelectedOption { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }

    public class TestResultResponse
    {
        public Guid SessionId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public int Score { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ViolationCount { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }
}
