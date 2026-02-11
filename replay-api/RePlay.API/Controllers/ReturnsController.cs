using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/returns")]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IReturnService _returnService;
    private readonly ILogger<ReturnsController> _logger;

    public ReturnsController(IReturnService returnService, ILogger<ReturnsController> logger)
    {
        _returnService = returnService;
        _logger = logger;
    }

    /// <summary>
    /// Initiate a toy return request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReturnResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReturnResult>> InitiateReturn([FromBody] CreateReturnDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _returnService.InitiateReturnAsync(dto, userId);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Created($"/api/v1/returns/{result.Return!.Id}", result);
    }

    /// <summary>
    /// Get current user's returns (paginated with filters)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ReturnDto>>> GetUserReturns([FromQuery] ReturnQueryParameters parameters)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _returnService.GetUserReturnsAsync(userId, parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get all returns - Admin only (paginated with filters)
    /// </summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PagedResult<ReturnDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ReturnDto>>> GetAllReturns([FromQuery] ReturnQueryParameters parameters)
    {
        var result = await _returnService.GetAllReturnsAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Approve a return with condition re-rating (Admin only)
    /// </summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ReturnResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReturnResult>> ApproveReturn(Guid id, [FromBody] ApproveReturnDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _returnService.ApproveReturnAsync(id, dto, adminId);

        if (!result.Succeeded)
        {
            if (result.Message == "Return not found.")
                return NotFound(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// Reject a return with notes (Admin only)
    /// </summary>
    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ReturnResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ReturnResult>> RejectReturn(Guid id, [FromBody] RejectReturnRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AdminNotes))
            return BadRequest(new { message = "Admin notes are required when rejecting a return." });

        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _returnService.RejectReturnAsync(id, request.AdminNotes, adminId);

        if (!result.Succeeded)
        {
            if (result.Message == "Return not found.")
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

public class RejectReturnRequest
{
    [Required]
    [StringLength(500)]
    public string AdminNotes { get; set; } = string.Empty;
}
