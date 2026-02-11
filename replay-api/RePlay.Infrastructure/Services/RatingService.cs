using Microsoft.EntityFrameworkCore;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly AppDbContext _context;

    public RatingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RatingDto> CreateRatingAsync(CreateRatingDto dto, Guid adminId)
    {
        var toyReturn = await _context.ToyReturns
            .Include(r => r.ReturnedByUser)
            .FirstOrDefaultAsync(r => r.Id == dto.ToyReturnId);

        if (toyReturn == null)
            throw new ArgumentException("Return not found.");

        if (toyReturn.Status != ReturnStatus.Approved)
            throw new InvalidOperationException("Can only rate users for approved returns.");

        if (dto.RatedUserId != toyReturn.ReturnedByUserId)
            throw new ArgumentException("Rated user must be the user who returned the toy.");

        var alreadyRated = await _context.Ratings
            .AnyAsync(r => r.ToyReturnId == dto.ToyReturnId);

        if (alreadyRated)
            throw new InvalidOperationException("This return has already been rated.");

        if (dto.Score < 1 || dto.Score > 5)
            throw new ArgumentException("Score must be between 1 and 5.");

        var rating = new Rating
        {
            Id = Guid.NewGuid(),
            RatedUserId = dto.RatedUserId,
            RatedByAdminId = adminId,
            ToyReturnId = dto.ToyReturnId,
            Score = dto.Score,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        await RecalculateAndUpdateReputationAsync(dto.RatedUserId);

        var created = await _context.Ratings
            .Include(r => r.RatedUser)
            .Include(r => r.RatedByAdmin)
            .FirstAsync(r => r.Id == rating.Id);

        return MapToDto(created);
    }

    public async Task<List<RatingDto>> GetUserRatingsAsync(Guid userId)
    {
        var ratings = await _context.Ratings
            .Include(r => r.RatedUser)
            .Include(r => r.RatedByAdmin)
            .Where(r => r.RatedUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ratings.Select(MapToDto).ToList();
    }

    public async Task<decimal> GetUserReputationScoreAsync(Guid userId)
    {
        var scores = await _context.Ratings
            .Where(r => r.RatedUserId == userId)
            .Select(r => r.Score)
            .ToListAsync();

        if (scores.Count == 0)
            return 0;

        return Math.Round((decimal)scores.Average(), 2);
    }

    /// <summary>
    /// Recalculates the user's reputation from all ratings and updates the User entity.
    /// </summary>
    internal async Task RecalculateAndUpdateReputationAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        var average = await GetUserReputationScoreAsync(userId);
        user.ReputationScore = average;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static RatingDto MapToDto(Rating r)
    {
        return new RatingDto
        {
            Id = r.Id,
            RatedUserId = r.RatedUserId,
            RatedUserName = r.RatedUser?.FullName ?? string.Empty,
            RatedByAdminId = r.RatedByAdminId,
            RatedByAdminName = r.RatedByAdmin?.FullName ?? string.Empty,
            ToyReturnId = r.ToyReturnId,
            Score = r.Score,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        };
    }
}
