namespace TestManagementApplication.Models.Entities
{
    public class VideoRecording
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Screen / Webcam
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public int ChunkIndex { get; set; } = 0;
        public bool IsComplete { get; set; } = false;

        // Navigation
        public TestSession? Session { get; set; }
    }

    public static class VideoType
    {
        public const string Screen = "Screen";
        public const string Webcam = "Webcam";
    }
}
