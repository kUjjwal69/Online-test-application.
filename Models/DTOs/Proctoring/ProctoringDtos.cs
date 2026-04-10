namespace TestManagementApplication.Models.DTOs.Proctoring
{
    public class UploadScreenshotRequest
    {
        public string ImageBase64 { get; set; } = string.Empty; // Base64-encoded image data
    }

    public class UploadScreenshotResponse
    {
        public Guid ImageId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime CapturedAt { get; set; }
    }

    public class ReportViolationRequest
    {
        public string ViolationType { get; set; } = string.Empty; // TabSwitch, WindowBlur, FullscreenExit
        public string? Details { get; set; }
    }

    public class ReportViolationResponse
    {
        public Guid ViolationId { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public int TotalViolations { get; set; }
        public bool SessionSuspended { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class VideoChunkUploadResponse
    {
        public Guid RecordingId { get; set; }
        public int ChunkIndex { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class VideoCompleteRequest
    {
        public Guid RecordingId { get; set; }
    }

    public class VideoCompleteResponse
    {
        public Guid RecordingId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class SessionHeartbeatRequest
    {
        /// <summary>Client UTC time when heartbeat was sent (optional).</summary>
        public DateTime? ClientTimeUtc { get; set; }

        /// <summary>Whether the exam page is currently in fullscreen mode (optional).</summary>
        public bool? IsFullscreen { get; set; }

        /// <summary>Whether the exam tab is visible (optional).</summary>
        public bool? IsTabVisible { get; set; }
    }

    public class SessionHeartbeatResponse
    {
        public Guid SessionId { get; set; }
        public DateTime ServerTimeUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ViolationCount { get; set; }
    }
}
