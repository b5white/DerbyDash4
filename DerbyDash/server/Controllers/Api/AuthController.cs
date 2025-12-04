using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using DerbyDash.Data;
using DerbyDash.Services;
using DerbyDash.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Controllers.Api {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IRecaptchaService _recaptchaService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtService jwtService,
            IUserService userService,
            IRecaptchaService recaptchaService,
            ILogger<AuthController> logger) {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _userService = userService;
            _recaptchaService = recaptchaService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request) {
            try {
                if (!ModelState.IsValid) {
                    return BadRequest(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Verify reCAPTCHA token if provided
                if (!string.IsNullOrWhiteSpace(request.RecaptchaToken)) {
                    var isHuman = await _recaptchaService.VerifyTokenAsync(request.RecaptchaToken);
                    if (!isHuman) {
                        return Unauthorized(new ApiResponse<LoginResponse> {
                            Success = false,
                            Message = "reCAPTCHA verification failed. Please try again."
                        });
                    }
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null) {
                    return Unauthorized(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded) {
                    return Unauthorized(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                var token = await _jwtService.GenerateTokenAsync(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

                var response = new LoginResponse {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600, // 1 hour
                    User = new UserDto {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        UserName = user.UserName ?? "",
                        EmailConfirmed = user.EmailConfirmed
                    }
                };

                return Ok(new ApiResponse<LoginResponse> {
                    Success = true,
                    Data = response,
                    Message = "Login successful"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new ApiResponse<LoginResponse> {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request) {
            try {
                if (!ModelState.IsValid) {
                    return BadRequest(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Verify reCAPTCHA token if provided
                if (!string.IsNullOrWhiteSpace(request.RecaptchaToken)) {
                    var isHuman = await _recaptchaService.VerifyTokenAsync(request.RecaptchaToken);
                    if (!isHuman) {
                        return BadRequest(new ApiResponse<LoginResponse> {
                            Success = false,
                            Message = "reCAPTCHA verification failed. Please try again."
                        });
                    }
                }

                if (request.Password != request.ConfirmPassword) {
                    return BadRequest(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "Passwords do not match"
                    });
                }

                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null) {
                    return BadRequest(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "User with this email already exists"
                    });
                }

                var user = new ApplicationUser {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true // For demo purposes
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded) {
                    return BadRequest(new ApiResponse<LoginResponse> {
                        Success = false,
                        Message = "Failed to create user",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    });
                }

                // Generate tokens for immediate login
                var token = await _jwtService.GenerateTokenAsync(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken);

                var response = new LoginResponse {
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600,
                    User = new UserDto {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        UserName = user.UserName ?? "",
                        EmailConfirmed = user.EmailConfirmed
                    }
                };

                return Ok(new ApiResponse<LoginResponse> {
                    Success = true,
                    Data = response,
                    Message = "Registration successful"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new ApiResponse<LoginResponse> {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request) {
            try {
                if (string.IsNullOrEmpty(request.RefreshToken)) {
                    return BadRequest(new ApiResponse<RefreshTokenResponse> {
                        Success = false,
                        Message = "Refresh token is required"
                    });
                }

                // Extract user ID from the expired access token
                var principal = _jwtService.ValidateToken(request.AccessToken);
                if (principal == null) {
                    return Unauthorized(new ApiResponse<RefreshTokenResponse> {
                        Success = false,
                        Message = "Invalid access token"
                    });
                }

                var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) {
                    return Unauthorized(new ApiResponse<RefreshTokenResponse> {
                        Success = false,
                        Message = "Invalid token claims"
                    });
                }

                var isValidRefreshToken = await _jwtService.ValidateRefreshTokenAsync(request.RefreshToken, userId);
                if (!isValidRefreshToken) {
                    return Unauthorized(new ApiResponse<RefreshTokenResponse> {
                        Success = false,
                        Message = "Invalid refresh token"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) {
                    return Unauthorized(new ApiResponse<RefreshTokenResponse> {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Generate new tokens
                var newAccessToken = await _jwtService.GenerateTokenAsync(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                
                // Revoke old refresh token and save new one
                await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken);
                await _jwtService.SaveRefreshTokenAsync(userId, newRefreshToken);

                var response = new RefreshTokenResponse {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    TokenType = "Bearer",
                    ExpiresIn = 3600
                };

                return Ok(new ApiResponse<RefreshTokenResponse> {
                    Success = true,
                    Data = response,
                    Message = "Token refreshed successfully"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new ApiResponse<RefreshTokenResponse> {
                    Success = false,
                    Message = "An error occurred during token refresh"
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] LogoutRequest request) {
            try {
                if (!string.IsNullOrEmpty(request.RefreshToken)) {
                    await _jwtService.RevokeRefreshTokenAsync(request.RefreshToken);
                }

                return Ok(new ApiResponse<bool> {
                    Success = true,
                    Data = true,
                    Message = "Logout successful"
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse<bool> {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser() {
            try {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) {
                    return Unauthorized(new ApiResponse<UserDto> {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) {
                    return NotFound(new ApiResponse<UserDto> {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var userDto = new UserDto {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    EmailConfirmed = user.EmailConfirmed
                };

                return Ok(new ApiResponse<UserDto> {
                    Success = true,
                    Data = userDto
                });
            } catch (Exception ex) {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new ApiResponse<UserDto> {
                    Success = false,
                    Message = "An error occurred while getting user information"
                });
            }
        }
    }
}
