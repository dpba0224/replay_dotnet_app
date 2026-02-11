namespace RePlay.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterDto dto);
    Task<AuthResult> VerifyEmailAsync(string email, string code);
    Task<AuthResult> LoginAsync(LoginDto dto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<AuthResult> ForgotPasswordAsync(string email);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task<ProfileUpdateResult> UpdateProfileAsync(Guid userId, UpdateProfileDto dto);
    Task<ProfileUpdateResult> UpdateProfileImageAsync(Guid userId, Stream imageStream, string fileName, string contentType);
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static AuthResult Success(string? message = null, string? accessToken = null, string? refreshToken = null, UserDto? user = null)
        => new() { Succeeded = true, Message = message, AccessToken = accessToken, RefreshToken = refreshToken, User = user };

    public static AuthResult Failure(string message, IEnumerable<string>? errors = null)
        => new() { Succeeded = false, Message = message, Errors = errors };
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public decimal ReputationScore { get; set; }
    public int TotalTradesCompleted { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
}

public class ProfileUpdateResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public UserDto? User { get; set; }

    public static ProfileUpdateResult Success(UserDto user, string? message = null)
        => new() { Succeeded = true, User = user, Message = message };

    public static ProfileUpdateResult Failure(string message)
        => new() { Succeeded = false, Message = message };
}
