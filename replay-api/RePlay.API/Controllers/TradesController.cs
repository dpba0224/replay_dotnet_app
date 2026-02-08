using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/trades")]
[Authorize]
public class TradesController : ControllerBase
{
    private readonly ITradeService _tradeService;
    private readonly ILogger<TradesController> _logger;

    public TradesController(ITradeService tradeService, ILogger<TradesController> logger)
    {
        _tradeService = tradeService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new trade request (trade or purchase)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TradeResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TradeResult>> CreateTrade([FromBody] CreateTradeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _tradeService.CreateTradeAsync(dto, userId);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return CreatedAtAction(nameof(GetTradeById), new { id = result.Trade!.Id }, result);
    }

    /// <summary>
    /// Get current user's trades (paginated with filters)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<TradeDto>>> GetUserTrades([FromQuery] TradeQueryParameters parameters)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _tradeService.GetUserTradesAsync(userId, parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get all trades - Admin only (paginated with filters)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<TradeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<TradeDto>>> GetAllTrades([FromQuery] TradeQueryParameters parameters)
    {
        var result = await _tradeService.GetAllTradesAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific trade by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TradeDto>> GetTradeById(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var trade = await _tradeService.GetTradeByIdAsync(id, userId);

        if (trade == null)
            return NotFound(new { message = "Trade not found" });

        // Non-admin users can only see their own trades
        if (!User.IsInRole("Admin") && trade.User.Id != userId)
            return NotFound(new { message = "Trade not found" });

        return Ok(trade);
    }

    /// <summary>
    /// Approve a trade request (Admin only)
    /// </summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TradeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TradeResult>> ApproveTrade(Guid id)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _tradeService.ApproveTradeAsync(id, adminId);

        if (!result.Succeeded)
        {
            if (result.Message == "Trade not found.")
                return NotFound(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// Cancel a pending trade request
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(TradeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TradeResult>> CancelTrade(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _tradeService.CancelTradeAsync(id, userId);

        if (!result.Succeeded)
        {
            if (result.Message == "Trade not found.")
                return NotFound(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
