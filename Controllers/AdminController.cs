using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagementApplication.Common;
using TestManagementApplication.Helpers;
using TestManagementApplication.Models.DTOs.Admin;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ══════════════════════════════════════════════════════
        //  TESTS
        // ══════════════════════════════════════════════════════

        /// <summary>[Admin] Get all tests</summary>
        [HttpGet("tests")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TestResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTests()
        {
            var tests = await _adminService.GetAllTestsAsync();
            return Ok(ApiResponse<IEnumerable<TestResponse>>.Ok(tests));
        }

        /// <summary>[Admin] Create a new test</summary>
        [HttpPost("tests")]
        [ProducesResponseType(typeof(ApiResponse<TestResponse>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateTest([FromBody] CreateTestRequest request)
        {
            var adminId = JwtHelper.GetUserIdFromClaims(User);
            var test = await _adminService.CreateTestAsync(request, adminId);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<TestResponse>.Ok(test, "Test created successfully."));
        }

        /// <summary>[Admin] Update an existing test</summary>
        [HttpPut("tests/{testId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTest(Guid testId, [FromBody] UpdateTestRequest request)
        {
            var test = await _adminService.UpdateTestAsync(testId, request);
            return Ok(ApiResponse<TestResponse>.Ok(test, "Test updated successfully."));
        }

        /// <summary>[Admin] Delete a test</summary>
        [HttpDelete("tests/{testId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTest(Guid testId)
        {
            await _adminService.DeleteTestAsync(testId);
            return Ok(ApiResponse.Ok("Test deleted successfully."));
        }

        // ══════════════════════════════════════════════════════
        //  QUESTIONS
        // ══════════════════════════════════════════════════════

        /// <summary>[Admin] Get all questions for a test</summary>
        [HttpGet("tests/{testId:guid}/questions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<QuestionResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuestions(Guid testId)
        {
            var questions = await _adminService.GetQuestionsByTestAsync(testId);
            return Ok(ApiResponse<IEnumerable<QuestionResponse>>.Ok(questions));
        }

        /// <summary>[Admin] Add a question to a test</summary>
        [HttpPost("tests/{testId:guid}/questions")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddQuestion(Guid testId, [FromBody] CreateQuestionRequest request)
        {
            var question = await _adminService.AddQuestionAsync(testId, request);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<QuestionResponse>.Ok(question, "Question added successfully."));
        }

        /// <summary>[Admin] Update a question</summary>
        [HttpPut("questions/{questionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<QuestionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionRequest request)
        {
            var question = await _adminService.UpdateQuestionAsync(questionId, request);
            return Ok(ApiResponse<QuestionResponse>.Ok(question, "Question updated successfully."));
        }

        /// <summary>[Admin] Delete a question</summary>
        [HttpDelete("questions/{questionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
        {
            await _adminService.DeleteQuestionAsync(questionId);
            return Ok(ApiResponse.Ok("Question deleted successfully."));
        }

        // ══════════════════════════════════════════════════════
        //  ASSIGNMENTS
        // ══════════════════════════════════════════════════════

        /// <summary>[Admin] Assign a test to a candidate</summary>
        [HttpPost("tests/{testId:guid}/assign")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignTest(Guid testId, [FromBody] AssignTestRequest request)
        {
            await _adminService.AssignTestAsync(testId, request);
            return Ok(ApiResponse.Ok("Test assigned to candidate successfully."));
        }

        // ══════════════════════════════════════════════════════
        //  SESSIONS
        // ══════════════════════════════════════════════════════

        /// <summary>[Admin] Get all test sessions</summary>
        [HttpGet("sessions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionSummaryResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSessions()
        {
            var sessions = await _adminService.GetAllSessionsAsync();
            return Ok(ApiResponse<IEnumerable<SessionSummaryResponse>>.Ok(sessions));
        }

        /// <summary>[Admin] Get a specific test session by ID</summary>
        [HttpGet("sessions/{sessionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SessionSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            var session = await _adminService.GetSessionByIdAsync(sessionId);
            return Ok(ApiResponse<SessionSummaryResponse>.Ok(session));
        }

        /// <summary>[Admin] Manually suspend an active test session</summary>
        [HttpPost("sessions/{sessionId:guid}/suspend")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SuspendSession(Guid sessionId)
        {
            await _adminService.SuspendSessionAsync(sessionId);
            return Ok(ApiResponse.Ok("Session suspended successfully."));
        }

        /// <summary>[Admin] Get all violations for a session</summary>
        [HttpGet("sessions/{sessionId:guid}/violations")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ViolationResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetViolations(Guid sessionId)
        {
            var violations = await _adminService.GetSessionViolationsAsync(sessionId);
            return Ok(ApiResponse<IEnumerable<ViolationResponse>>.Ok(violations));
        }

        /// <summary>[Admin] Get all screenshots captured during a session</summary>
        [HttpGet("sessions/{sessionId:guid}/screenshots")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ScreenshotResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetScreenshots(Guid sessionId)
        {
            var screenshots = await _adminService.GetSessionScreenshotsAsync(sessionId);
            return Ok(ApiResponse<IEnumerable<ScreenshotResponse>>.Ok(screenshots));
        }

        /// <summary>[Admin] Get all uploaded video recordings for a session</summary>
        [HttpGet("sessions/{sessionId:guid}/UploadedVideos")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VideoRecordingResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVideos(Guid sessionId)
        {
            var videos = await _adminService.GetSessionVideosAsync(sessionId);
            return Ok(ApiResponse<IEnumerable<VideoRecordingResponse>>.Ok(videos));
        }

        // ══════════════════════════════════════════════════════
        //  USERS
        // ══════════════════════════════════════════════════════

        /// <summary>[Admin] Get all registered candidates</summary>
        [HttpGet("Get-all-users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCandidates()
        {
            var users = await _adminService.GetAllCandidatesAsync();
            return Ok(ApiResponse<IEnumerable<UserResponse>>.Ok(users));
        }
    }
}
