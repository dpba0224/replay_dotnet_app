using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/toys")]
public class ToysController : ControllerBase
{
    private readonly IToyService _toyService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<ToysController> _logger;

    public ToysController(
        IToyService toyService,
        IFileUploadService fileUploadService,
        ILogger<ToysController> logger)
    {
        _toyService = toyService;
        _fileUploadService = fileUploadService;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of toys with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ToyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ToyDto>>> GetToys([FromQuery] ToyQueryParameters parameters)
    {
        // Non-admin users cannot see archived toys
        if (!User.IsInRole("Admin"))
        {
            parameters.IncludeArchived = false;
        }

        var result = await _toyService.GetToysAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific toy by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ToyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToyDto>> GetToyById(Guid id)
    {
        var toy = await _toyService.GetToyByIdAsync(id);

        if (toy == null)
            return NotFound(new { message = "Toy not found" });

        // Non-admin users cannot see archived toys
        if (toy.IsArchived && !User.IsInRole("Admin"))
            return NotFound(new { message = "Toy not found" });

        return Ok(toy);
    }

    /// <summary>
    /// Get a specific toy by shareable slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ToyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToyDto>> GetToyBySlug(string slug)
    {
        var toy = await _toyService.GetToyBySlugAsync(slug);

        if (toy == null)
            return NotFound(new { message = "Toy not found" });

        if (toy.IsArchived && !User.IsInRole("Admin"))
            return NotFound(new { message = "Toy not found" });

        return Ok(toy);
    }

    /// <summary>
    /// Create a new toy (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ToyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToyDto>> CreateToy([FromBody] CreateToyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var toy = await _toyService.CreateToyAsync(dto, adminId);

        return CreatedAtAction(nameof(GetToyById), new { id = toy.Id }, toy);
    }

    /// <summary>
    /// Update an existing toy (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ToyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToyDto>> UpdateToy(Guid id, [FromBody] UpdateToyDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var toy = await _toyService.UpdateToyAsync(id, dto);

        if (toy == null)
            return NotFound(new { message = "Toy not found" });

        return Ok(toy);
    }

    /// <summary>
    /// Archive a toy (soft delete, Admin only)
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveToy(Guid id)
    {
        var result = await _toyService.ArchiveToyAsync(id);

        if (!result)
            return NotFound(new { message = "Toy not found" });

        return Ok(new { message = "Toy archived successfully" });
    }

    /// <summary>
    /// Restore an archived toy (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RestoreToy(Guid id)
    {
        var result = await _toyService.RestoreToyAsync(id);

        if (!result)
            return NotFound(new { message = "Toy not found" });

        return Ok(new { message = "Toy restored successfully" });
    }

    /// <summary>
    /// Upload an image for a toy (Admin only)
    /// </summary>
    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ToyImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ToyImageDto>> UploadToyImage(Guid id, IFormFile file, [FromQuery] int displayOrder = 1)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        // Verify toy exists
        var toy = await _toyService.GetToyByIdAsync(id);
        if (toy == null)
            return NotFound(new { message = "Toy not found" });

        try
        {
            await using var stream = file.OpenReadStream();
            var imagePath = await _fileUploadService.UploadImageAsync(stream, file.FileName, file.ContentType);

            var resultPath = await _toyService.AddToyImageAsync(id, imagePath, displayOrder);

            return Created(resultPath, new ToyImageDto
            {
                ImagePath = resultPath,
                DisplayOrder = displayOrder
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete an image from a toy (Admin only)
    /// </summary>
    [HttpDelete("{toyId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteToyImage(Guid toyId, Guid imageId)
    {
        var result = await _toyService.RemoveToyImageAsync(toyId, imageId);

        if (!result)
            return NotFound(new { message = "Image not found" });

        return Ok(new { message = "Image deleted successfully" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
