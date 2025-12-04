using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DerbyDash.Shared.DTOs;
using DerbyDash.Shared.Models;
using DerbyDash.Services;
using DerbyDash.Data;
using DerbyDash.Exceptions;

namespace DerbyDash.Controllers.Api {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "InternalApiPolicy")]
    public class RacersController : ControllerBase {
        private readonly IRaceTeamService _raceTeamService;
        private readonly IUserService _userService;
        private readonly ILogger<RacersController> _logger;

        public RacersController(
            IRaceTeamService raceTeamService,
            IUserService userService,
            ILogger<RacersController> logger) {
            _raceTeamService = raceTeamService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<RacerDto>>>> GetRacers() {
            try {
                var racers = await _raceTeamService.GetRacers(true);
                var racerDtos = racers.Select(r => new RacerDto {
                    Id = r.Id,
                    UserId = r.UserId,
                    Name = r.Name,
                    LastRaced = r.LastRaced,
                    LastPlayedRace = r.LastPlayedRace,
                    AvatarFileName = r.AvatarFileName,
                    RaceCount = r.RaceCount
                }).ToList();

                return Ok(new ApiResponse<List<RacerDto>> {
                    Success = true,
                    Data = racerDtos
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting racers");
                return StatusCode(500, new ApiResponse<List<RacerDto>> {
                    Success = false,
                    Message = "Error retrieving racers"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<RacerDto>>> GetRacer(int id) {
            try {
                var racer = await _raceTeamService.GetRacerByIdAsync(id);
                if (racer == null) {
                    return NotFound(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                var racerDto = new RacerDto {
                    Id = racer.Id,
                    UserId = racer.UserId,
                    Name = racer.Name,
                    LastRaced = racer.LastRaced,
                    LastPlayedRace = racer.LastPlayedRace,
                    AvatarFileName = racer.AvatarFileName,
                    RaceCount = racer.RaceCount
                };

                return Ok(new ApiResponse<RacerDto> {
                    Success = true,
                    Data = racerDto
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting racer {RacerId}", id);
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = "Error retrieving racer"
                });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<RacerDto?>>> GetActiveRacer() {
            try {
                var racer = await _raceTeamService.GetActiveRacer();
                RacerDto? racerDto = null;
                
                if (racer != null) {
                    racerDto = new RacerDto {
                        Id = racer.Id,
                        UserId = racer.UserId,
                        Name = racer.Name,
                        LastRaced = racer.LastRaced,
                        LastPlayedRace = racer.LastPlayedRace,
                        AvatarFileName = racer.AvatarFileName,
                        RaceCount = racer.RaceCount
                    };
                }

                return Ok(new ApiResponse<RacerDto?> {
                    Success = true,
                    Data = racerDto
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting active racer");
                return StatusCode(500, new ApiResponse<RacerDto?> {
                    Success = false,
                    Message = "Error retrieving active racer"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RacerDto>>> CreateRacer([FromBody] CreateRacerRequest request) {
            try {
                if (string.IsNullOrWhiteSpace(request.Name)) {
                    return BadRequest(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "Racer name is required"
                    });
                }

                var newRacer = new Racer {
                    Name = request.Name.Trim(),
                    AvatarFileName = request.AvatarFileName
                };

                var createdRacer = await _raceTeamService.AddRacer(newRacer);
                var racerDto = new RacerDto {
                    Id = createdRacer.Id,
                    UserId = createdRacer.UserId,
                    Name = createdRacer.Name,
                    LastRaced = createdRacer.LastRaced,
                    LastPlayedRace = createdRacer.LastPlayedRace,
                    AvatarFileName = createdRacer.AvatarFileName,
                    RaceCount = createdRacer.RaceCount
                };

                return CreatedAtAction(nameof(GetRacer), new { id = createdRacer.Id }, 
                    new ApiResponse<RacerDto> {
                        Success = true,
                        Data = racerDto
                    });
            } catch (MissingUserException ex) {
                _logger.LogWarning(ex, "User authentication issue when creating racer");
                return Unauthorized(new ApiResponse<RacerDto> {
                    Success = false,
                    Message = ex.Message
                });
            } catch (InvalidOperationException ex) {
                _logger.LogWarning(ex, "Invalid operation when creating racer");
                return BadRequest(new ApiResponse<RacerDto> {
                    Success = false,
                    Message = ex.Message
                });
            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Database error when creating racer: {ErrorMessage}", ex.Message);
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = $"Database error: {innerMessage}"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating racer: {ErrorMessage}. Type: {ExceptionType}", ex.Message, ex.GetType().Name);
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = $"Error creating racer: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RacerDto>>> UpdateRacer(int id, [FromBody] UpdateRacerRequest request) {
            try {
                if (request.Id != id) {
                    return BadRequest(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "ID mismatch"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Name)) {
                    return BadRequest(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "Racer name is required"
                    });
                }

                var existingRacer = await _raceTeamService.GetRacerByIdAsync(id);
                if (existingRacer == null) {
                    return NotFound(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                existingRacer.Name = request.Name.Trim();
                existingRacer.AvatarFileName = request.AvatarFileName;

                await _raceTeamService.UpdateRacer(existingRacer);

                var racerDto = new RacerDto {
                    Id = existingRacer.Id,
                    UserId = existingRacer.UserId,
                    Name = existingRacer.Name,
                    LastRaced = existingRacer.LastRaced,
                    LastPlayedRace = existingRacer.LastPlayedRace,
                    AvatarFileName = existingRacer.AvatarFileName,
                    RaceCount = existingRacer.RaceCount
                };

                return Ok(new ApiResponse<RacerDto> {
                    Success = true,
                    Data = racerDto
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error updating racer {RacerId}", id);
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = "Error updating racer"
                });
            }
        }

        [HttpPost("{id}/set-active")]
        public async Task<ActionResult<ApiResponse<bool>>> SetActiveRacer(int id) {
            try {
                var racer = await _raceTeamService.GetRacerByIdAsync(id);
                if (racer == null) {
                    return NotFound(new ApiResponse<bool> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                await _raceTeamService.SetActiveRacer(racer);

                return Ok(new ApiResponse<bool> {
                    Success = true,
                    Data = true,
                    Message = $"Set {racer.Name} as active racer"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error setting active racer {RacerId}", id);
                return StatusCode(500, new ApiResponse<bool> {
                    Success = false,
                    Message = "Error setting active racer"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteRacer(int id) {
            try {
                var racer = await _raceTeamService.GetRacerByIdAsync(id);
                if (racer == null) {
                    return NotFound(new ApiResponse<bool> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                await _raceTeamService.RemoveRacer(id);

                return Ok(new ApiResponse<bool> {
                    Success = true,
                    Data = true,
                    Message = "Racer deleted successfully"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting racer {RacerId}", id);
                return StatusCode(500, new ApiResponse<bool> {
                    Success = false,
                    Message = "Error deleting racer"
                });
            }
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<object>>> GetRacerStats() {
            try {
                var teamRaceCount = await _raceTeamService.GetTeamRaceCountAsync();
                var currentRacerRaceCount = await _raceTeamService.GetCurrentRacerRaceCountAsync();
                var lastPlayedRace = await _raceTeamService.GetLastPlayedRaceAsync();

                var stats = new {
                    TeamRaceCount = teamRaceCount,
                    CurrentRacerRaceCount = currentRacerRaceCount,
                    LastPlayedRace = lastPlayedRace
                };

                return Ok(new ApiResponse<object> {
                    Success = true,
                    Data = stats
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting racer stats");
                return StatusCode(500, new ApiResponse<object> {
                    Success = false,
                    Message = "Error retrieving stats"
                });
            }
        }
    }
}
