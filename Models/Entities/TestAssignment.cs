namespace TestManagementApplication.Models.Entities
{
    public class TestAssignment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TestId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        // Navigation
        public Test? Test { get; set; }
        public User? User { get; set; }
    }
}
