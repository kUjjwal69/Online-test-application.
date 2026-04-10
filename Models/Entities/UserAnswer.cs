namespace TestManagementApplication.Models.Entities
{
    public class UserAnswer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public Guid QuestionId { get; set; }
        public string SelectedOption { get; set; } = string.Empty; // A, B, C, D
        public bool IsCorrect { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public TestSession? Session { get; set; }
        public Question? Question { get; set; }
    }
}
