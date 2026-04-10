namespace TestManagementApplication.Models.Entities
{
    public class Violation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public string ViolationType { get; set; } = string.Empty; // TabSwitch, WindowBlur, FullscreenExit
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }

        // Navigation
        public TestSession? Session { get; set; }
    }

    public static class ViolationType
    {
        public const string TabSwitch = "TabSwitch";
        public const string WindowBlur = "WindowBlur";
        public const string FullscreenExit = "FullscreenExit";
    }
}
