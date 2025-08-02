using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DerbyDash.Shared.Models;
using DerbyDash.Shared.DTOs;
using DerbyDash.Services;
using DerbyDash.Data;

namespace DerbyDash.Controllers.Api {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RacersController : ControllerBase {
        private readonly IRaceTeamService _raceTeamService;
        private readonly ILogger<RacersController> _logger;

        public RacersController(IRaceTeamService raceTeamService, ILogger<RacersController> logger) {
            _raceTeamService = raceTeamService;
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
                _logger.LogError(ex, "Error getting racer {Id}", id);
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = "Error retrieving racer"
                });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<RacerDto>>> GetActiveRacer() {
            try {
                var racer = await _raceTeamService.GetActiveRacer();
                if (racer == null) {
                    return Ok(new ApiResponse<RacerDto> {
                        Success = true,
                        Data = null,
                        Message = "No active racer"
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
                _logger.LogError(ex, "Error getting active racer");
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = "Error retrieving active racer"
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<RacerDto>>> CreateRacer([FromBody] CreateRacerRequest request) {
            try {
                var racer = new Racer {
                    Name = request.Name,
                    AvatarFileName = request.AvatarFileName
                };

                var createdRacer = await _raceTeamService.AddRacer(racer);

                var racerDto = new RacerDto {
                    Id = createdRacer.Id,
                    UserId = createdRacer.UserId,
                    Name = createdRacer.Name,
                    LastRaced = createdRacer.LastRaced,
                    LastPlayedRace = createdRacer.LastPlayedRace,
                    AvatarFileName = createdRacer.AvatarFileName,
                    RaceCount = createdRacer.RaceCount
                };

                return CreatedAtAction(nameof(GetRacer), new { id = racerDto.Id }, new ApiResponse<RacerDto> {
                    Success = true,
                    Data = racerDto
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating racer");
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = "Error creating racer"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<RacerDto>>> UpdateRacer(int id, [FromBody] UpdateRacerRequest request) {
            try {
                if (id != request.Id) {
                    return BadRequest(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "ID mismatch"
                    });
                }

                var existingRacer = await _raceTeamService.GetRacerByIdAsync(id);
                if (existingRacer == null) {
                    return NotFound(new ApiResponse<RacerDto> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                existingRacer.Name = request.Name;
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
                _logger.LogError(ex, "Error updating racer {Id}", id);
                return StatusCode(500, new ApiResponse<RacerDto> {
                    Success = false,
                    Message = "Error updating racer"
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRacer(int id) {
            try {
                var racer = await _raceTeamService.GetRacerByIdAsync(id);
                if (racer == null) {
                    return NotFound(new ApiResponse<object> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                await _raceTeamService.RemoveRacer(id);

                return Ok(new ApiResponse<object> {
                    Success = true,
                    Message = "Racer deleted successfully"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting racer {Id}", id);
                return StatusCode(500, new ApiResponse<object> {
                    Success = false,
                    Message = "Error deleting racer"
                });
            }
        }

        [HttpPost("{id}/set-active")]
        public async Task<ActionResult<ApiResponse<object>>> SetActiveRacer(int id) {
            try {
                var racer = await _raceTeamService.GetRacerByIdAsync(id);
                if (racer == null) {
                    return NotFound(new ApiResponse<object> {
                        Success = false,
                        Message = "Racer not found"
                    });
                }

                await _raceTeamService.SetActiveRacer(racer);

                return Ok(new ApiResponse<object> {
                    Success = true,
                    Message = "Active racer set successfully"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error setting active racer {Id}", id);
                return StatusCode(500, new ApiResponse<object> {
                    Success = false,
                    Message = "Error setting active racer"
                });
            }
        }
    }
}
