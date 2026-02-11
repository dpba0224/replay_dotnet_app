using System.ComponentModel.DataAnnotations;

namespace RePlay.Application.Interfaces;

public interface IRatingService
{
    Task<RatingDto> CreateRatingAsync(CreateRatingDto dto, Guid adminId);
    Task<List<RatingDto>> GetUserRatingsAsync(Guid userId);
    Task<decimal> GetUserReputationScoreAsync(Guid userId);
}

public class CreateRatingDto
{
    [Required]
    public Guid RatedUserId { get; set; }

    [Required]
    public Guid ToyReturnId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Score { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }
}

public class RatingDto
{
    public Guid Id { get; set; }
    public Guid RatedUserId { get; set; }
    public string RatedUserName { get; set; } = string.Empty;
    public Guid RatedByAdminId { get; set; }
    public string RatedByAdminName { get; set; } = string.Empty;
    public Guid ToyReturnId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
