namespace TestManagementApplication.Models.Entities
{
    public class Question
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TestId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty; // A, B, C, D
        public int Marks { get; set; } = 1;
        public int OrderIndex { get; set; }

        // Navigation
        public Test? Test { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
