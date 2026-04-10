using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagementApplication.Common;
using TestManagementApplication.Helpers;
using TestManagementApplication.Models.DTOs.Candidate;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Controllers
{
    [ApiController]
    [Route("api/candidate")]
    [Authorize(Roles = "User")]
    [Produces("application/json")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidateController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        // ══════════════════════════════════════════════════════
        //  ASSIGNED TESTS
        // ══════════════════════════════════════════════════════

        /// <summary>[Candidate] View all tests assigned to the logged-in candidate</summary>
        [HttpGet("tests")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AssignedTestResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAssignedTests()
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var tests = await _candidateService.GetAssignedTestsAsync(userId);
            return Ok(ApiResponse<IEnumerable<AssignedTestResponse>>.Ok(tests));
        }

        // ══════════════════════════════════════════════════════
        //  START TEST
        // ══════════════════════════════════════════════════════

        /// <summary>[Candidate] Start an assigned test (creates a TestSession)</summary>
        [HttpPost("tests/{testId:guid}/start")]
        [ProducesResponseType(typeof(ApiResponse<StartTestResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StartTest(Guid testId)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _candidateService.StartTestAsync(testId, userId);
            return Ok(ApiResponse<StartTestResponse>.Ok(result, "Test session started."));
        }

        // ══════════════════════════════════════════════════════
        //  QUESTIONS
        // ══════════════════════════════════════════════════════

        /// <summary>[Candidate] Fetch questions for an active test session</summary>
        [HttpGet("sessions/{sessionId:guid}/questions")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<QuestionForCandidateResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetQuestions(Guid sessionId)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var questions = await _candidateService.GetQuestionsAsync(sessionId, userId);
            return Ok(ApiResponse<IEnumerable<QuestionForCandidateResponse>>.Ok(questions));
        }

        // ══════════════════════════════════════════════════════
        //  SUBMIT ANSWER
        // ══════════════════════════════════════════════════════

        /// <summary>[Candidate] Submit or update an answer for a question</summary>
        [HttpPost("sessions/{sessionId:guid}/answers")]
        [ProducesResponseType(typeof(ApiResponse<SubmitAnswerResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitAnswer(Guid sessionId, [FromBody] SubmitAnswerRequest request)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _candidateService.SubmitAnswerAsync(sessionId, userId, request);
            return Ok(ApiResponse<SubmitAnswerResponse>.Ok(result, "Answer submitted."));
        }

        // ══════════════════════════════════════════════════════
        //  SUBMIT TEST
        // ══════════════════════════════════════════════════════

        /// <summary>[Candidate] Submit the test and get scored results</summary>
        [HttpPost("sessions/{sessionId:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<TestResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitTest(Guid sessionId)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _candidateService.SubmitTestAsync(sessionId, userId);
            return Ok(ApiResponse<TestResultResponse>.Ok(result, "Test submitted successfully."));
        }

        // ══════════════════════════════════════════════════════
        //  GET RESULT
        // ══════════════════════════════════════════════════════

        /// <summary>[Candidate] View results of a completed test session</summary>
        [HttpGet("sessions/{sessionId:guid}/result")]
        [ProducesResponseType(typeof(ApiResponse<TestResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetResult(Guid sessionId)
        {
            var userId = JwtHelper.GetUserIdFromClaims(User);
            var result = await _candidateService.GetResultAsync(sessionId, userId);
            return Ok(ApiResponse<TestResultResponse>.Ok(result));
        }
    }
}
