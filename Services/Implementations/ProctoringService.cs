using TestManagementApplication.Models.DTOs.Proctoring;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Interfaces;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Services.Implementations
{
    public class ProctoringService : IProctoringService
    {
        private readonly ITestSessionRepository _sessionRepo;
        private readonly IViolationRepository _violationRepo;
        private readonly ICapturedImageRepository _imageRepo;
        private readonly IVideoRecordingRepository _videoRepo;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ProctoringService> _logger;

        public ProctoringService(
            ITestSessionRepository sessionRepo,
            IViolationRepository violationRepo,
            ICapturedImageRepository imageRepo,
            IVideoRecordingRepository videoRepo,
            IWebHostEnvironment env,
            ILogger<ProctoringService> logger)
        {
            _sessionRepo = sessionRepo;
            _violationRepo = violationRepo;
            _imageRepo = imageRepo;
            _videoRepo = videoRepo;
            _env = env;
            _logger = logger;
        }

        // ─── SCREENSHOT ───────────────────────────────────────────────────

        public async Task<UploadScreenshotResponse> UploadScreenshotAsync(
            Guid sessionId, Guid userId, UploadScreenshotRequest request)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);

            if (string.IsNullOrWhiteSpace(request.ImageBase64))
                throw new ArgumentException("Image data is required.");

            // Strip base64 prefix if present (e.g., "data:image/png;base64,...")
            var base64Data = request.ImageBase64;
            if (base64Data.Contains(','))
                base64Data = base64Data.Split(',')[1];

            try
            {
                var imageBytes = Convert.FromBase64String(base64Data);
                var screenshotDir = Path.Combine(_env.WebRootPath, "uploads", "screenshots", sessionId.ToString());
                Directory.CreateDirectory(screenshotDir);

                var fileName = $"{Guid.NewGuid()}.png";
                var filePath = Path.Combine(screenshotDir, fileName);
                await File.WriteAllBytesAsync(filePath, imageBytes);

                var relativePath = $"/uploads/screenshots/{sessionId}/{fileName}";

                var captured = new CapturedImage
                {
                    SessionId = sessionId,
                    FilePath = relativePath,
                    CapturedAt = DateTime.UtcNow
                };

                await _imageRepo.AddAsync(captured);

                return new UploadScreenshotResponse
                {
                    ImageId = captured.Id,
                    FilePath = relativePath,
                    CapturedAt = captured.CapturedAt
                };
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid Base64 image data.");
            }
        }

        // ─── VIOLATION ────────────────────────────────────────────────────

        public async Task<ReportViolationResponse> ReportViolationAsync(
            Guid sessionId, Guid userId, ReportViolationRequest request)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);

            var validTypes = new[] { ViolationType.TabSwitch, ViolationType.WindowBlur, ViolationType.FullscreenExit };
            if (!validTypes.Contains(request.ViolationType))
                throw new ArgumentException($"ViolationType must be one of: {string.Join(", ", validTypes)}");

            var violation = new Violation
            {
                SessionId = sessionId,
                ViolationType = request.ViolationType,
                Details = request.Details,
                OccurredAt = DateTime.UtcNow
            };

            await _violationRepo.AddAsync(violation);

            // Increment violation count on session
            session.ViolationCount++;
            bool suspended = false;

            // Fetch the test to get the threshold
            var totalViolations = await _violationRepo.CountBySessionIdAsync(sessionId);
            var testThreshold = session.Test?.ViolationThreshold ?? 3;

            if (totalViolations >= testThreshold)
            {
                session.Status = SessionStatus.Suspended;
                session.EndTime = DateTime.UtcNow;
                suspended = true;
                _logger.LogWarning("Session {SessionId} suspended due to {Count} violations.", sessionId, totalViolations);
            }

            await _sessionRepo.UpdateAsync(session);

            return new ReportViolationResponse
            {
                ViolationId = violation.Id,
                ViolationType = violation.ViolationType,
                TotalViolations = (int)totalViolations,
                SessionSuspended = suspended,
                Message = suspended
                    ? "Your test session has been suspended due to repeated violations."
                    : $"Violation recorded. You have {totalViolations} of {testThreshold} allowed violations."
            };
        }

        // ─── VIDEO CHUNK UPLOAD ───────────────────────────────────────────

        public async Task<VideoChunkUploadResponse> UploadVideoChunkAsync(
            Guid sessionId, Guid userId, IFormFile chunk, string videoType, int chunkIndex, Guid? recordingId)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);

            var validTypes = new[] { VideoType.Screen, VideoType.Webcam };
            if (!validTypes.Contains(videoType))
                throw new ArgumentException($"VideoType must be Screen or Webcam.");

            if (chunk == null || chunk.Length == 0)
                throw new ArgumentException("Chunk file is required and cannot be empty.");

            // Max chunk size: 50 MB
            const long maxChunkSize = 50 * 1024 * 1024;
            if (chunk.Length > maxChunkSize)
                throw new ArgumentException("Chunk size exceeds maximum allowed 50 MB.");

            VideoRecording recording;

            if (recordingId.HasValue)
            {
                recording = await _videoRepo.GetByIdAsync(recordingId.Value)
                    ?? throw new KeyNotFoundException($"Recording {recordingId} not found.");

                if (recording.SessionId != sessionId)
                    throw new UnauthorizedAccessException("Recording does not belong to this session.");
                if (!string.Equals(recording.Type, videoType, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("VideoType does not match the existing recording type.");
                if (recording.IsComplete)
                    throw new InvalidOperationException("Recording is already completed.");
                if (chunkIndex < 0)
                    throw new ArgumentException("chunkIndex must be >= 0.");
                if (chunkIndex <= recording.ChunkIndex)
                    throw new InvalidOperationException("chunkIndex must be greater than the last uploaded chunkIndex.");
            }
            else
            {
                // First chunk — create new recording entry
                if (chunkIndex != 0)
                    throw new ArgumentException("First chunk must have chunkIndex = 0.");

                recording = new VideoRecording
                {
                    SessionId = sessionId,
                    Type = videoType,
                    StartTime = DateTime.UtcNow,
                    ChunkIndex = 0,
                    IsComplete = false,
                    FilePath = string.Empty // will be set on complete
                };
                await _videoRepo.AddAsync(recording);
            }

            // Save chunk to disk
            var chunkDir = Path.Combine(_env.WebRootPath, "uploads", "videos", sessionId.ToString(), recording.Id.ToString(), "chunks");
            Directory.CreateDirectory(chunkDir);

            var chunkFileName = $"chunk_{chunkIndex:D6}.part";
            var chunkPath = Path.Combine(chunkDir, chunkFileName);

            using (var stream = new FileStream(chunkPath, FileMode.Create, FileAccess.Write))
            {
                await chunk.CopyToAsync(stream);
            }

            recording.ChunkIndex = chunkIndex;
            await _videoRepo.UpdateAsync(recording);

            return new VideoChunkUploadResponse
            {
                RecordingId = recording.Id,
                ChunkIndex = chunkIndex,
                Type = videoType,
                Message = $"Chunk {chunkIndex} uploaded successfully."
            };
        }

        // ─── VIDEO COMPLETE ───────────────────────────────────────────────

        public async Task<VideoCompleteResponse> CompleteVideoAsync(
            Guid sessionId, Guid userId, VideoCompleteRequest request)
        {
            await ValidateActiveSessionAsync(sessionId, userId);

            var recording = await _videoRepo.GetByIdAsync(request.RecordingId)
                ?? throw new KeyNotFoundException($"Recording {request.RecordingId} not found.");

            if (recording.SessionId != sessionId)
                throw new UnauthorizedAccessException("Recording does not belong to this session.");

            if (recording.IsComplete)
                throw new InvalidOperationException("Recording is already completed.");

            // Concatenate chunks into a single video file
            var chunkDir = Path.Combine(_env.WebRootPath, "uploads", "videos", sessionId.ToString(), recording.Id.ToString(), "chunks");
            var finalDir = Path.Combine(_env.WebRootPath, "uploads", "videos", sessionId.ToString());
            Directory.CreateDirectory(finalDir);

            var finalFileName = $"{recording.Id}_{recording.Type}.webm";
            var finalPath = Path.Combine(finalDir, finalFileName);

            if (Directory.Exists(chunkDir))
            {
                var chunkFiles = Directory.GetFiles(chunkDir, "chunk_*")
                    .OrderBy(f => f)
                    .ToArray();

                if (chunkFiles.Length == 0)
                    throw new InvalidOperationException("No chunks found to complete the recording.");

                using var finalStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write);
                foreach (var chunkFile in chunkFiles)
                {
                    using var chunkStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read);
                    await chunkStream.CopyToAsync(finalStream);
                }

                // Clean up chunks after merging
                Directory.Delete(chunkDir, recursive: true);
            }
            else
            {
                throw new InvalidOperationException("Chunk directory not found. Upload chunks before completing.");
            }

            var relativePath = $"/uploads/videos/{sessionId}/{finalFileName}";
            recording.FilePath = relativePath;
            recording.EndTime = DateTime.UtcNow;
            recording.IsComplete = true;

            await _videoRepo.UpdateAsync(recording);

            return new VideoCompleteResponse
            {
                RecordingId = recording.Id,
                FilePath = relativePath,
                Type = recording.Type,
                StartTime = recording.StartTime,
                EndTime = recording.EndTime!.Value
            };
        }

        // ─── HEARTBEAT ────────────────────────────────────────────────────

        public async Task<SessionHeartbeatResponse> HeartbeatAsync(
            Guid sessionId,
            Guid userId,
            string? ip,
            string? userAgent,
            SessionHeartbeatRequest request)
        {
            var session = await ValidateActiveSessionAsync(sessionId, userId);

            session.LastHeartbeatAt = DateTime.UtcNow;
            session.LastClientTimeUtc = request.ClientTimeUtc;
            session.LastKnownIp = string.IsNullOrWhiteSpace(ip) ? session.LastKnownIp : ip;
            session.LastKnownUserAgent = string.IsNullOrWhiteSpace(userAgent) ? session.LastKnownUserAgent : userAgent;
            session.LastKnownFullscreen = request.IsFullscreen;
            session.LastKnownTabVisible = request.IsTabVisible;

            await _sessionRepo.UpdateAsync(session);

            return new SessionHeartbeatResponse
            {
                SessionId = session.Id,
                ServerTimeUtc = DateTime.UtcNow,
                Status = session.Status,
                ViolationCount = session.ViolationCount
            };
        }

        // ─── PRIVATE HELPERS ──────────────────────────────────────────────

        private async Task<TestSession> ValidateActiveSessionAsync(Guid sessionId, Guid userId)
        {
            var session = await _sessionRepo.GetByIdAsync(sessionId)
                ?? throw new KeyNotFoundException($"Session {sessionId} not found.");

            if (session.UserId != userId)
                throw new UnauthorizedAccessException("You do not have access to this session.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException($"Session is {session.Status}. Proctoring actions not allowed.");

            return session;
        }
    }
}
