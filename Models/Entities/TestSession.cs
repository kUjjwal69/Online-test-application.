namespace TestManagementApplication.Models.Entities
{
    public class TestSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TestId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = SessionStatus.Active; // Active / Completed / Suspended
        public int ViolationCount { get; set; } = 0;
        public int? Score { get; set; }
        public decimal? Percentage { get; set; }
        public bool? IsPassed { get; set; }

        // Monitoring (updated by proctoring heartbeat)
        public DateTime? LastHeartbeatAt { get; set; }
        public DateTime? LastClientTimeUtc { get; set; }
        public string? LastKnownIp { get; set; }
        public string? LastKnownUserAgent { get; set; }
        public bool? LastKnownFullscreen { get; set; }
        public bool? LastKnownTabVisible { get; set; }

        // Navigation
        public Test? Test { get; set; }
        public User? User { get; set; }
        public ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
        public ICollection<CapturedImage> CapturedImages { get; set; } = new List<CapturedImage>();
        public ICollection<VideoRecording> VideoRecordings { get; set; } = new List<VideoRecording>();
    }

    public static class SessionStatus
    {
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Suspended = "Suspended";
    }
}
