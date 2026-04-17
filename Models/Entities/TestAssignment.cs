namespace TestManagementApplication.Models.Entities
{
    public class TestAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TestId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public string Status { get; set; } = AssignmentStatus.NotStarted;

        // Navigation
        public Test? Test { get; set; }
        public User? User { get; set; }
    }

    public static class AssignmentStatus
    {
        public const string NotStarted = "NotStarted";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
        public const string Suspended = "Suspended";
    }
}
