namespace TestManagementApplication.Models.Entities
{
    public class CapturedImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public TestSession? Session { get; set; }
    }
}
