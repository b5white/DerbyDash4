using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Problems;
using DerbyDash.Problems;
using DerbyDash.Services;
using DerbyDash.Data;
using DerbyDash.DTOs;

namespace DerbyDash.Controllers.Api {
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ProblemsController : ControllerBase {
        private readonly ILogger<ProblemsController> _logger;
        private readonly IFAQService _faqService;

        public ProblemsController(ILogger<ProblemsController> logger, IFAQService faqService) {
            _logger = logger;
            _faqService = faqService;
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

        [HttpGet("faq")]
        public async Task<ActionResult<ApiResponse<List<FAQDto>>>> GetFAQs() {
            try {
                var faqs = await _faqService.GetAllFAQsAsync();
                var faqDtos = faqs.Select(f => new FAQDto {
                    Category = f.Category,
                    Question = f.Question,
                    Answer = f.Answer,
                    IsActive = true,
                    Order = 0
                }).ToList();

                return Ok(new ApiResponse<List<FAQDto>> {
                    Success = true,
                    Data = faqDtos
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting FAQs");
                return StatusCode(500, new ApiResponse<List<FAQDto>> {
                    Success = false,
                    Message = "Error retrieving FAQs"
                });
            }
        }

        [HttpGet("faq/categories")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetFAQCategories() {
            try {
                var categories = await _faqService.GetCategoriesAsync();
                return Ok(new ApiResponse<List<string>> {
                    Success = true,
                    Data = categories
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting FAQ categories");
                return StatusCode(500, new ApiResponse<List<string>> {
                    Success = false,
                    Message = "Error retrieving FAQ categories"
                });
            }
        }

        [HttpGet("faq/category/{category}")]
        public async Task<ActionResult<ApiResponse<List<FAQDto>>>> GetFAQsByCategory(string category) {
            try {
                var faqs = await _faqService.GetFAQsByCategoryAsync(category);
                var faqDtos = faqs.Select(f => new FAQDto {
                    Category = f.Category,
                    Question = f.Question,
                    Answer = f.Answer,
                    IsActive = true,
                    Order = 0
                }).ToList();

                return Ok(new ApiResponse<List<FAQDto>> {
                    Success = true,
                    Data = faqDtos
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting FAQs for category: {Category}", category);
                return StatusCode(500, new ApiResponse<List<FAQDto>> {
                    Success = false,
                    Message = $"Error retrieving FAQs for category: {category}"
                });
            }
        }
    }
}
