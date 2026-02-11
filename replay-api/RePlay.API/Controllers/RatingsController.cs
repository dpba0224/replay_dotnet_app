using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/ratings")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;
    private readonly ILogger<RatingsController> _logger;

    public RatingsController(IRatingService ratingService, ILogger<RatingsController> logger)
    {
        _ratingService = ratingService;
        _logger = logger;
    }

    /// <summary>
    /// Create a rating for a user (admin only, linked to an approved return).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(RatingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RatingDto>> CreateRating([FromBody] CreateRatingDto dto)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        try
        {
            var rating = await _ratingService.CreateRatingAsync(dto, adminId);
            return Created($"/api/v1/ratings/user/{dto.RatedUserId}", rating);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all ratings for a user.
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(List<RatingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<RatingDto>>> GetUserRatings(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && currentUserId != userId)
            return Forbid();

        var ratings = await _ratingService.GetUserRatingsAsync(userId);
        return Ok(ratings);
    }

    /// <summary>
    /// Get a user's reputation score (average of all ratings).
    /// </summary>
    [HttpGet("user/{userId:guid}/reputation")]
    [ProducesResponseType(typeof(ReputationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReputationResponse>> GetUserReputation(Guid userId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && currentUserId != userId)
            return Forbid();

        var score = await _ratingService.GetUserReputationScoreAsync(userId);
        return Ok(new ReputationResponse { ReputationScore = score });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class ReputationResponse
{
    public decimal ReputationScore { get; set; }
}
