using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using DerbyDash.Data;

namespace DerbyDash.Services {
    public interface IJwtService {
        Task<string> GenerateTokenAsync(ApplicationUser user);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId);
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }

    public class JwtService : IJwtService {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<JwtService> _logger;

        public JwtService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<JwtService> logger) {
            _configuration = configuration;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user) {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "DerbyDash_Super_Secret_Key_That_Is_At_Least_32_Characters_Long";
            var issuer = jwtSettings["Issuer"] ?? "DerbyDash";
            var audience = jwtSettings["Audience"] ?? "DerbyDashAPI";
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            };

            // Add user roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles) {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token) {
            try {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? "DerbyDash_Super_Secret_Key_That_Is_At_Least_32_Characters_Long";
                var issuer = jwtSettings["Issuer"] ?? "DerbyDash";
                var audience = jwtSettings["Audience"] ?? "DerbyDashAPI";

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            } catch (Exception ex) {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public string GenerateRefreshToken() {
            var randomBytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId) {
            // In a real application, you would store refresh tokens in the database
            // For this demo, we'll use a simple validation
            return !string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(userId);
        }

        public async Task SaveRefreshTokenAsync(string userId, string refreshToken) {
            // In a real application, save to database
            // For demo purposes, we'll just log it
            _logger.LogInformation("Refresh token saved for user {UserId}", userId);
            await Task.CompletedTask;
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken) {
            // In a real application, mark the refresh token as revoked in database
            _logger.LogInformation("Refresh token revoked: {RefreshToken}", refreshToken);
            await Task.CompletedTask;
        }
    }
}
