using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(new RegisterDto
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        });

        if (!result.Succeeded)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Title = "Registration Failed",
                Message = result.Message,
                Errors = result.Errors?.ToDictionary(e => "Error", e => new[] { e })
            });
        }

        return Ok(new AuthResponse { Message = result.Message });
    }

    /// <summary>
    /// Verify email with the code sent to user's email
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request.Email, request.Code);

        if (!result.Succeeded)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Title = "Verification Failed",
                Message = result.Message
            });
        }

        return Ok(new AuthResponse
        {
            Message = result.Message,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            User = result.User
        });
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = request.Email,
            Password = request.Password
        });

        if (!result.Succeeded)
        {
            return Unauthorized(new ErrorResponse
            {
                Status = 401,
                Title = "Authentication Failed",
                Message = result.Message
            });
        }

        return Ok(new AuthResponse
        {
            Message = result.Message,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            User = result.User
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (!result.Succeeded)
        {
            return Unauthorized(new ErrorResponse
            {
                Status = 401,
                Title = "Token Refresh Failed",
                Message = result.Message
            });
        }

        return Ok(new AuthResponse
        {
            Message = result.Message,
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            User = result.User
        });
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email);
        return Ok(new AuthResponse { Message = result.Message });
    }

    /// <summary>
    /// Reset password with the code sent to user's email
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(new ResetPasswordDto
        {
            Token = request.Token,
            NewPassword = request.NewPassword
        });

        if (!result.Succeeded)
        {
            return BadRequest(new ErrorResponse
            {
                Status = 400,
                Title = "Password Reset Failed",
                Message = result.Message,
                Errors = result.Errors?.ToDictionary(e => "Error", e => new[] { e })
            });
        }

        return Ok(new AuthResponse { Message = result.Message });
    }

    /// <summary>
    /// Get current authenticated user's information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [DisableRateLimiting]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    /// <summary>
    /// Update current user's profile (name)
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [DisableRateLimiting]
    [ProducesResponseType(typeof(ProfileUpdateResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _authService.UpdateProfileAsync(userId, new UpdateProfileDto
        {
            FullName = request.FullName
        });

        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Upload or update current user's profile image
    /// </summary>
    [HttpPost("profile/image")]
    [Authorize]
    [DisableRateLimiting]
    [ProducesResponseType(typeof(ProfileUpdateResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfileImage(IFormFile file)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _authService.UpdateProfileImageAsync(userId, stream, file.FileName, file.ContentType);

            if (!result.Succeeded)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

// Request DTOs
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class VerifyEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;
}

// Response DTOs
public class AuthResponse
{
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public UserDto? User { get; set; }
}

public class UpdateProfileRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public int Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
