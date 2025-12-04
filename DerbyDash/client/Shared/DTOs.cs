namespace DerbyDash.Shared.DTOs {
    // API Request/Response DTOs
    public class CreateRacerRequest {
        public string Name { get; set; } = string.Empty;
        public string? AvatarFileName { get; set; }
    }

    public class UpdateRacerRequest {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? AvatarFileName { get; set; }
    }

    public class SaveRaceRequest {
        public double TotalTime { get; set; }
        public string ProblemClassString { get; set; } = string.Empty;
        public List<SpeedIncrementData>? SpeedIncrements { get; set; }
        public int FinishingPosition { get; set; } = 0;
    }

    public class SpeedIncrementData {
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
    }

    public class SubmitFeedbackRequest {
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool CanReply { get; set; } = false;
        public string? ContactEmail { get; set; }
    }

    public class ApiResponse<T> {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class PaginatedResponse<T> {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    // Authentication DTOs
    public class LoginRequest {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? RecaptchaToken { get; set; }
    }

    public class RegisterRequest {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? RecaptchaToken { get; set; }
    }

    public class LoginResponse {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public UserDto? User { get; set; }
    }

    public class RefreshTokenRequest {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenResponse {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }

    public class LogoutRequest {
        public string? RefreshToken { get; set; }
    }

    public class UserDto {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
    }
}
