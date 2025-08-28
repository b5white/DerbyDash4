using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;
using DerbyDash.Services;
using DerbyDash.Data;

namespace DerbyDash.Controllers.Api {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "InternalApiPolicy")]
    public class RaceController : ControllerBase {
        private readonly RaceService _raceService;
        private readonly IRaceTeamService _raceTeamService;
        private readonly IUserService _userService;
        private readonly ILogger<RaceController> _logger;

        public RaceController(
            RaceService raceService,
            IRaceTeamService raceTeamService,
            IUserService userService,
            ILogger<RaceController> logger) {
            _raceService = raceService;
            _raceTeamService = raceTeamService;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("save")]
        public async Task<ActionResult<ApiResponse<RaceDto>>> SaveRace([FromBody] SaveRaceRequest request) {
            try {
                var activeRacer = await _raceTeamService.GetActiveRacer();
                if (activeRacer == null) {
                    return BadRequest(new ApiResponse<RaceDto> {
                        Success = false,
                        Message = "No active racer found. Please select an active racer first."
                    });
                }

                // Create race record using the existing service method
                var speedIncrements = request.SpeedIncrements?.Select(si => new SpeedIncrement {
                    Time = si.Time,
                    Speed = si.Speed,
                    Distance = si.Distance
                }).ToList();

                var savedRace = await _raceTeamService.SaveRaceCompletionAsync(
                    request.TotalTime, 
                    request.ProblemClassString, 
                    speedIncrements, 
                    request.FinishingPosition);

                // Also update the last played race
                await _raceTeamService.SaveLastPlayedRaceAsync(request.ProblemClassString);

                var raceDto = new RaceDto {
                    Id = savedRace.Id,
                    RacerId = savedRace.RacerId,
                    RaceDateTime = savedRace.RaceDateTime,
                    TotalTime = savedRace.TotalTime,
                    ProblemSetId = savedRace.ProblemSetId,
                    FinishingPosition = savedRace.FinishingPosition,
                    SpeedIncrements = savedRace.SpeedIncrements?.Select(si => new SpeedIncrementDto {
                        Id = si.Id,
                        RaceId = si.RaceId,
                        Time = si.Time,
                        Speed = si.Speed,
                        Distance = si.Distance
                    }).ToList()
                };

                return Ok(new ApiResponse<RaceDto> {
                    Success = true,
                    Data = raceDto,
                    Message = "Race saved successfully"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error saving race");
                return StatusCode(500, new ApiResponse<RaceDto> {
                    Success = false,
                    Message = "Error saving race"
                });
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<RaceDto>>>> GetRaceHistory(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? racerId = null) {
            try {
                if (pageSize > 100) pageSize = 100; // Limit page size
                
                var races = await _raceService.GetRaceHistoryAsync(page, pageSize, racerId);
                var totalCount = await _raceService.GetRaceCountAsync(racerId);

                var raceDtos = races.Select(r => new RaceDto {
                    Id = r.Id,
                    RacerId = r.RacerId,
                    RaceDateTime = r.RaceDateTime,
                    TotalTime = r.TotalTime,
                    ProblemSetId = r.ProblemSetId,
                    FinishingPosition = r.FinishingPosition,
                    SpeedIncrements = r.SpeedIncrements?.Select(si => new SpeedIncrementDto {
                        Id = si.Id,
                        RaceId = si.RaceId,
                        Time = si.Time,
                        Speed = si.Speed,
                        Distance = si.Distance
                    }).ToList()
                }).ToList();

                var paginatedResponse = new PaginatedResponse<RaceDto> {
                    Items = raceDtos,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize
                };

                return Ok(new ApiResponse<PaginatedResponse<RaceDto>> {
                    Success = true,
                    Data = paginatedResponse
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting race history");
                return StatusCode(500, new ApiResponse<PaginatedResponse<RaceDto>> {
                    Success = false,
                    Message = "Error retrieving race history"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RaceDto>>> GetRace(int id) {
            try {
                var race = await _raceService.GetRaceByIdAsync(id);
                if (race == null) {
                    return NotFound(new ApiResponse<RaceDto> {
                        Success = false,
                        Message = "Race not found"
                    });
                }

                var raceDto = new RaceDto {
                    Id = race.Id,
                    RacerId = race.RacerId,
                    RaceDateTime = race.RaceDateTime,
                    TotalTime = race.TotalTime,
                    ProblemSetId = race.ProblemSetId,
                    FinishingPosition = race.FinishingPosition,
                    SpeedIncrements = race.SpeedIncrements?.Select(si => new SpeedIncrementDto {
                        Id = si.Id,
                        RaceId = si.RaceId,
                        Time = si.Time,
                        Speed = si.Speed,
                        Distance = si.Distance
                    }).ToList()
                };

                return Ok(new ApiResponse<RaceDto> {
                    Success = true,
                    Data = raceDto
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting race {RaceId}", id);
                return StatusCode(500, new ApiResponse<RaceDto> {
                    Success = false,
                    Message = "Error retrieving race"
                });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetRaceStats([FromQuery] int? racerId = null) {
            try {
                var stats = await _raceService.GetRaceStatsAsync(racerId);
                
                return Ok(new ApiResponse<object> {
                    Success = true,
                    Data = stats
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting race stats");
                return StatusCode(500, new ApiResponse<object> {
                    Success = false,
                    Message = "Error retrieving race statistics"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRace(int id) {
            try {
                var race = await _raceService.GetRaceByIdAsync(id);
                if (race == null) {
                    return NotFound(new ApiResponse<bool> {
                        Success = false,
                        Message = "Race not found"
                    });
                }

                await _raceService.DeleteRaceAsync(id);

                return Ok(new ApiResponse<bool> {
                    Success = true,
                    Data = true,
                    Message = "Race deleted successfully"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting race {RaceId}", id);
                return StatusCode(500, new ApiResponse<bool> {
                    Success = false,
                    Message = "Error deleting race"
                });
            }
        }

        private int GetProblemSetId(string problemClassString) {
            // Convert problem class string to problem set ID
            // This maps to the existing problem set logic
            return problemClassString switch {
                "addition-4stable" => 1,
                "addition-6stable" => 2,
                "subtraction-4stable" => 3,
                "subtraction-6stable" => 4,
                "multiplication-4stable" => 5,
                "multiplication-6stable" => 6,
                "division-4stable" => 7,
                "division-6stable" => 8,
                "mixed-4stable" => 9,
                "mixed-6stable" => 10,
                _ => 0
            };
        }
    }
}
