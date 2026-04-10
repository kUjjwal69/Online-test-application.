namespace TestManagementApplication.Models.Entities
{
    public class Test
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int TotalMarks { get; set; }
        public int PassingMarks { get; set; }
        public int ViolationThreshold { get; set; } = 3;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }

        // Navigation
        public User? Creator { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();
        public ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    }
}
