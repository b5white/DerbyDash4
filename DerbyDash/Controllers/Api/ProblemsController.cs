using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Problems;
using DerbyDash.Components.Problems;

namespace DerbyDash.Controllers.Api {
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ProblemsController : ControllerBase {
        private readonly ILogger<ProblemsController> _logger;

        public ProblemsController(ILogger<ProblemsController> logger) {
            _logger = logger;
        }

        [HttpGet("{problemType}")]
        public ActionResult<ApiResponse<ProblemSetDto>> GetProblems(string problemType) {
            try {
                var problemManager = ProblemFactory.CreateProblemManager(problemType);
                problemManager.InitializeProblems();

                var problems = new List<ProblemDto>();
                while (problemManager.More) {
                    var problem = problemManager.Next();
                    problems.Add(new ProblemDto {
                        Title = problem.Title,
                        Description = problem.Description,
                        Result = problem.Result,
                        Length = problem.Length
                    });
                }

                var problemSet = new ProblemSetDto {
                    Title = problemManager.Title,
                    Problems = problems,
                    HasMore = false
                };

                return Ok(new ApiResponse<ProblemSetDto> {
                    Success = true,
                    Data = problemSet
                });
            } catch (ArgumentException ex) {
                _logger.LogWarning(ex, "Invalid problem type: {ProblemType}", problemType);
                return BadRequest(new ApiResponse<ProblemSetDto> {
                    Success = false,
                    Message = $"Invalid problem type: {problemType}"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting problems for type: {ProblemType}", problemType);
                return StatusCode(500, new ApiResponse<ProblemSetDto> {
                    Success = false,
                    Message = "Error retrieving problems"
                });
            }
        }

        [HttpGet("{problemType}/title")]
        public ActionResult<ApiResponse<string>> GetProblemTitle(string problemType) {
            try {
                var title = ProblemFactory.GetTitle(problemType);
                return Ok(new ApiResponse<string> {
                    Success = true,
                    Data = title
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting problem title for type: {ProblemType}", problemType);
                return StatusCode(500, new ApiResponse<string> {
                    Success = false,
                    Message = "Error retrieving problem title"
                });
            }
        }

        [HttpGet]
        public ActionResult<ApiResponse<List<string>>> GetAvailableProblemTypes() {
            try {
                // This could be made more dynamic by reflecting on available problem types
                var problemTypes = new List<string> {
                    "addition-2stable",
                    "addition-3stable",
                    "addition-4stable",
                    "addition-5stable",
                    "addition-1digitsimple",
                    "addition-1digit",
                    "multiplication-squaressmall"
                };

                return Ok(new ApiResponse<List<string>> {
                    Success = true,
                    Data = problemTypes
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting available problem types");
                return StatusCode(500, new ApiResponse<List<string>> {
                    Success = false,
                    Message = "Error retrieving problem types"
                });
            }
        }
    }
}
