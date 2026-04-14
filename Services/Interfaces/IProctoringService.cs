using TestManagementApplication.Models.DTOs.Proctoring;

namespace TestManagementApplication.Services.Interfaces
{
    public interface IProctoringService
    {
        Task<UploadScreenshotResponse> UploadScreenshotAsync(Guid sessionId, Guid userId, UploadScreenshotRequest request);
        Task<ReportViolationResponse> ReportViolationAsync(Guid sessionId, Guid userId, ReportViolationRequest request);
        Task<VideoChunkUploadResponse> UploadVideoChunkAsync(Guid sessionId, Guid userId, IFormFile chunk, string videoType, int chunkIndex, Guid? recordingId);
        Task<VideoCompleteResponse> CompleteVideoAsync(Guid sessionId, Guid userId, VideoCompleteRequest request);
        Task<SessionHeartbeatResponse> HeartbeatAsync(Guid sessionId, Guid userId, string? ip, string? userAgent, SessionHeartbeatRequest request);
    }
}
