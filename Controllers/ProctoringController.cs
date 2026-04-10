using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagementApplication.Common;
using TestManagementApplication.Helpers;
using TestManagementApplication.Models.DTOs.Proctoring;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Controllers
{
    [ApiController]
    [Route("api/proctoring")]
    [Authorize(Roles = "User")]
    [Produces("application/json")]
    public class ProctoringController : ControllerBase
    {
        private readonly IProctoringService _proctoringService;

        public ProctoringController(IProctoringService proctoringService)
        {
            _proctoringService = proctoringService;
        }

        // ══════════════════════════════════════════════════════
        //  SCREENSHOT UPLOAD
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// [Candidate] Upload a Base64-encoded screenshot for proctoring.
        /// The image is decoded on the server and saved to wwwroot/uploads/screenshots/{sessionId}/
        /// </summary>
        [HttpPost("sessions/{sessionId:guid}/screenshot")]
        [ProducesResponseType(typeof(ApiResponse<UploadScreenshotResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadScreenshot(
            Guid sessionId,
            [FromBody] UploadScreenshotRequest request)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _proctoringService.UploadScreenshotAsync(sessionId, userId, request);
            return Ok(ApiResponse<UploadScreenshotResponse>.Ok(result, "Screenshot uploaded."));
        }

        // ══════════════════════════════════════════════════════
        //  VIOLATION REPORTING
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// [Candidate] Report a proctoring violation event.
        /// Accepted types: TabSwitch, WindowBlur, FullscreenExit.
        /// Session is auto-suspended if violation count exceeds the test threshold.
        /// </summary>
        [HttpPost("sessions/{sessionId:guid}/violation")]
        [ProducesResponseType(typeof(ApiResponse<ReportViolationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReportViolation(
            Guid sessionId,
            [FromBody] ReportViolationRequest request)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _proctoringService.ReportViolationAsync(sessionId, userId, request);
            return Ok(ApiResponse<ReportViolationResponse>.Ok(result));
        }

        // ══════════════════════════════════════════════════════
        //  VIDEO CHUNK UPLOAD
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// [Candidate] Upload a video chunk (screen recording or webcam).
        /// Use multipart/form-data. Pass recordingId (from first response) for subsequent chunks.
        /// VideoType: Screen | Webcam. Max chunk size: 50 MB.
        /// </summary>
        [HttpPost("sessions/{sessionId:guid}/video/chunk")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<VideoChunkUploadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [RequestSizeLimit(52_428_800)] // 50 MB
        public async Task<IActionResult> UploadVideoChunk(
            Guid sessionId,
            [FromForm] IFormFile chunk,
            [FromForm] string videoType,
            [FromForm] int chunkIndex,
            [FromForm] Guid? recordingId = null)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _proctoringService.UploadVideoChunkAsync(
                sessionId, userId, chunk, videoType, chunkIndex, recordingId);
            return Ok(ApiResponse<VideoChunkUploadResponse>.Ok(result));
        }

        // ══════════════════════════════════════════════════════
        //  VIDEO COMPLETE
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// [Candidate] Signal that all chunks have been uploaded.
        /// Server merges all chunks into a single video file and stores the metadata.
        /// </summary>
        [HttpPost("sessions/{sessionId:guid}/video/complete")]
        [ProducesResponseType(typeof(ApiResponse<VideoCompleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteVideo(
            Guid sessionId,
            [FromBody] VideoCompleteRequest request)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _proctoringService.CompleteVideoAsync(sessionId, userId, request);
            return Ok(ApiResponse<VideoCompleteResponse>.Ok(result, "Video recording finalized."));
        }

        // ══════════════════════════════════════════════════════
        //  SESSION HEARTBEAT / MONITORING
        // ══════════════════════════════════════════════════════

        /// <summary>
        /// [Candidate] Send a heartbeat to keep the session monitored.
        /// Updates last-seen timestamps and basic client state (fullscreen/tab visibility).
        /// </summary>
        [HttpPost("sessions/{sessionId:guid}/heartbeat")]
        [ProducesResponseType(typeof(ApiResponse<SessionHeartbeatResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Heartbeat(
            Guid sessionId,
            [FromBody] SessionHeartbeatRequest request)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers.UserAgent.ToString();
            var result = await _proctoringService.HeartbeatAsync(sessionId, userId, ip, userAgent, request);
            return Ok(ApiResponse<SessionHeartbeatResponse>.Ok(result));
        }
    }
}
