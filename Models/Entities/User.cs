namespace TestManagementApplication.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Admin / User
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation
        public ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();
        public ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
        public ICollection<Test> CreatedTests { get; set; } = new List<Test>();
    }
}
