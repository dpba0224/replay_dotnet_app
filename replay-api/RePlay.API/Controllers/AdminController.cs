using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Get all users (paginated with search and filters)
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] UserQueryParameters parameters)
    {
        var result = await _adminService.GetUsersAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Activate a user account
    /// </summary>
    [HttpPatch("users/{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var result = await _adminService.ActivateUserAsync(id);

        if (!result)
            return NotFound(new { message = "User not found." });

        return Ok(new { message = "User activated successfully." });
    }

    /// <summary>
    /// Deactivate a user account
    /// </summary>
    [HttpPatch("users/{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var result = await _adminService.DeactivateUserAsync(id);

        if (!result)
            return NotFound(new { message = "User not found." });

        return Ok(new { message = "User deactivated successfully." });
    }
}
