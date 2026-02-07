using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class ToyService : IToyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ToyService> _logger;

    public ToyService(AppDbContext context, ILogger<ToyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<ToyDto>> GetToysAsync(ToyQueryParameters parameters)
    {
        var query = _context.Toys
            .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
            .AsQueryable();

        // Filter by archived status
        if (!parameters.IncludeArchived)
        {
            query = query.Where(t => !t.IsArchived);
        }

        // Filter by status
        if (parameters.Status.HasValue)
        {
            query = query.Where(t => t.Status == parameters.Status.Value);
        }

        // Filter by category
        if (parameters.Category.HasValue)
        {
            query = query.Where(t => t.Category == parameters.Category.Value);
        }

        // Filter by minimum condition
        if (parameters.MinCondition.HasValue)
        {
            query = query.Where(t => t.Condition >= parameters.MinCondition.Value);
        }

        // Filter by age group
        if (!string.IsNullOrWhiteSpace(parameters.AgeGroup))
        {
            query = query.Where(t => t.AgeGroup == parameters.AgeGroup);
        }

        // Filter by price range
        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(t => t.Price >= parameters.MinPrice.Value);
        }
        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(t => t.Price <= parameters.MaxPrice.Value);
        }

        // Search by name or description
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(searchTerm) ||
                t.Description.ToLower().Contains(searchTerm));
        }

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "oldest" => query.OrderBy(t => t.CreatedAt),
            "price_asc" => query.OrderBy(t => t.Price),
            "price_desc" => query.OrderByDescending(t => t.Price),
            "name" => query.OrderBy(t => t.Name),
            "condition" => query.OrderByDescending(t => t.Condition),
            _ => query.OrderByDescending(t => t.CreatedAt) // newest (default)
        };

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ToyDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ToyDto?> GetToyByIdAsync(Guid id)
    {
        var toy = await _context.Toys
            .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == id);

        return toy == null ? null : MapToDto(toy);
    }

    public async Task<ToyDto?> GetToyBySlugAsync(string slug)
    {
        var toy = await _context.Toys
            .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.ShareableSlug == slug);

        return toy == null ? null : MapToDto(toy);
    }

    public async Task<ToyDto> CreateToyAsync(CreateToyDto dto, Guid adminId)
    {
        var toy = new Toy
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            AgeGroup = dto.AgeGroup,
            Condition = dto.Condition,
            Price = dto.Price,
            Status = ToyStatus.Available,
            IsArchived = false,
            ShareableSlug = GenerateSlug(dto.Name),
            CreatedByAdminId = adminId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Toys.Add(toy);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new toy: {ToyName} (ID: {ToyId})", toy.Name, toy.Id);

        return MapToDto(toy);
    }

    public async Task<ToyDto?> UpdateToyAsync(Guid id, UpdateToyDto dto)
    {
        var toy = await _context.Toys
            .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == id);

        if (toy == null)
            return null;

        if (dto.Name != null)
        {
            toy.Name = dto.Name;
            toy.ShareableSlug = GenerateSlug(dto.Name);
        }
        if (dto.Description != null)
            toy.Description = dto.Description;
        if (dto.Category.HasValue)
            toy.Category = dto.Category.Value;
        if (dto.AgeGroup != null)
            toy.AgeGroup = dto.AgeGroup;
        if (dto.Condition.HasValue)
            toy.Condition = dto.Condition.Value;
        if (dto.Price.HasValue)
            toy.Price = dto.Price.Value;

        toy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated toy: {ToyName} (ID: {ToyId})", toy.Name, toy.Id);

        return MapToDto(toy);
    }

    public async Task<bool> ArchiveToyAsync(Guid id)
    {
        var toy = await _context.Toys.FindAsync(id);
        if (toy == null)
            return false;

        toy.IsArchived = true;
        toy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived toy: {ToyName} (ID: {ToyId})", toy.Name, toy.Id);

        return true;
    }

    public async Task<bool> RestoreToyAsync(Guid id)
    {
        var toy = await _context.Toys.FindAsync(id);
        if (toy == null)
            return false;

        toy.IsArchived = false;
        toy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Restored toy: {ToyName} (ID: {ToyId})", toy.Name, toy.Id);

        return true;
    }

    public async Task<string> AddToyImageAsync(Guid toyId, string imagePath, int displayOrder)
    {
        var toy = await _context.Toys.FindAsync(toyId);
        if (toy == null)
            throw new ArgumentException("Toy not found", nameof(toyId));

        var image = new ToyImage
        {
            Id = Guid.NewGuid(),
            ToyId = toyId,
            ImagePath = imagePath,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ToyImage>().Add(image);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added image to toy {ToyId}: {ImagePath}", toyId, imagePath);

        return imagePath;
    }

    public async Task<bool> RemoveToyImageAsync(Guid toyId, Guid imageId)
    {
        var image = await _context.Set<ToyImage>()
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ToyId == toyId);

        if (image == null)
            return false;

        _context.Set<ToyImage>().Remove(image);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed image {ImageId} from toy {ToyId}", imageId, toyId);

        return true;
    }

    private static ToyDto MapToDto(Toy toy)
    {
        return new ToyDto
        {
            Id = toy.Id,
            Name = toy.Name,
            Description = toy.Description,
            Category = toy.Category.ToString(),
            AgeGroup = toy.AgeGroup,
            Condition = (int)toy.Condition,
            ConditionLabel = toy.Condition.ToString(),
            Price = toy.Price,
            Status = toy.Status.ToString(),
            IsArchived = toy.IsArchived,
            ShareableSlug = toy.ShareableSlug,
            CreatedAt = toy.CreatedAt,
            Images = toy.Images.Select(i => new ToyImageDto
            {
                Id = i.Id,
                ImagePath = i.ImagePath,
                DisplayOrder = i.DisplayOrder
            }).ToList()
        };
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("&", "and");

        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove multiple consecutive dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        // Trim dashes from ends
        slug = slug.Trim('-');

        // Add random suffix to ensure uniqueness
        var suffix = Guid.NewGuid().ToString("N")[..6];
        return $"{slug}-{suffix}";
    }
}
