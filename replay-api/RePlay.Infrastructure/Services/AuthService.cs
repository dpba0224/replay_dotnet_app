using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        IEmailService emailService,
        IMemoryCache cache,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _emailService = emailService;
        _cache = cache;
        _context = context;
    }

    public async Task<AuthResult> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return AuthResult.Failure("A user with this email already exists.");
        }

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return AuthResult.Failure(
                "Registration failed.",
                result.Errors.Select(e => e.Description)
            );
        }

        await _userManager.AddToRoleAsync(user, "User");

        // Generate and cache verification code
        var verificationCode = GenerateVerificationCode();
        var cacheKey = $"email_verification_{dto.Email}";
        _cache.Set(cacheKey, verificationCode, TimeSpan.FromMinutes(15));

        // Send verification email
        await _emailService.SendEmailVerificationAsync(dto.Email, dto.FullName, verificationCode);

        return AuthResult.Success("Registration successful. Please check your email to verify your account.");
    }

    public async Task<AuthResult> VerifyEmailAsync(string email, string code)
    {
        var cacheKey = $"email_verification_{email}";
        if (!_cache.TryGetValue(cacheKey, out string? storedCode) || storedCode != code)
        {
            return AuthResult.Failure("Invalid or expired verification code.");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return AuthResult.Failure("User not found.");
        }

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _cache.Remove(cacheKey);

        // Auto-login after verification
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        await StoreRefreshTokenAsync(user.Id, refreshToken);

        var userDto = MapToUserDto(user, roles.FirstOrDefault() ?? "User");

        return AuthResult.Success(
            "Email verified successfully.",
            accessToken,
            refreshToken,
            userDto
        );
    }

    public async Task<AuthResult> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return AuthResult.Failure("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return AuthResult.Failure("Please verify your email before logging in.");
        }

        if (!user.IsActive)
        {
            return AuthResult.Failure("Your account has been deactivated. Please contact support.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return AuthResult.Failure("Account is locked. Please try again later.");
            }
            return AuthResult.Failure("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await StoreRefreshTokenAsync(user.Id, refreshToken);

        var userDto = MapToUserDto(user, roles.FirstOrDefault() ?? "User");

        return AuthResult.Success(
            "Login successful.",
            accessToken,
            refreshToken,
            userDto
        );
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // Validate refresh token exists in cache/storage
        var cacheKey = $"refresh_token_{refreshToken}";
        if (!_cache.TryGetValue(cacheKey, out Guid userId))
        {
            return AuthResult.Failure("Invalid or expired refresh token.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
        {
            return AuthResult.Failure("User not found or inactive.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Remove old refresh token and store new one
        _cache.Remove(cacheKey);
        await StoreRefreshTokenAsync(user.Id, newRefreshToken);

        var userDto = MapToUserDto(user, roles.FirstOrDefault() ?? "User");

        return AuthResult.Success(
            "Token refreshed successfully.",
            newAccessToken,
            newRefreshToken,
            userDto
        );
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that the user doesn't exist
            return AuthResult.Success("If an account with that email exists, we've sent a password reset code.");
        }

        var resetCode = GenerateVerificationCode();
        var cacheKey = $"password_reset_{email}";
        _cache.Set(cacheKey, resetCode, TimeSpan.FromMinutes(15));

        await _emailService.SendPasswordResetAsync(email, user.FullName, resetCode);

        return AuthResult.Success("If an account with that email exists, we've sent a password reset code.");
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        // First, find the user by checking all password reset cache entries
        // In production, you'd want a more robust approach
        var users = await _context.Users.ToListAsync();
        User? targetUser = null;
        string? email = null;

        foreach (var user in users)
        {
            var cacheKey = $"password_reset_{user.Email}";
            if (_cache.TryGetValue(cacheKey, out string? storedCode) && storedCode == dto.Token)
            {
                targetUser = user;
                email = user.Email;
                break;
            }
        }

        if (targetUser == null || email == null)
        {
            return AuthResult.Failure("Invalid or expired reset code.");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(targetUser);
        var result = await _userManager.ResetPasswordAsync(targetUser, resetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            return AuthResult.Failure(
                "Password reset failed.",
                result.Errors.Select(e => e.Description)
            );
        }

        _cache.Remove($"password_reset_{email}");

        return AuthResult.Success("Password has been reset successfully. You can now log in with your new password.");
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserDto(user, roles.FirstOrDefault() ?? "User");
    }

    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private async Task StoreRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var cacheKey = $"refresh_token_{refreshToken}";
        _cache.Set(cacheKey, userId, TimeSpan.FromDays(7));
        await Task.CompletedTask;
    }

    private static UserDto MapToUserDto(User user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = role,
            ProfileImageUrl = user.ProfileImageUrl,
            ReputationScore = user.ReputationScore,
            TotalTradesCompleted = user.TotalTradesCompleted,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
