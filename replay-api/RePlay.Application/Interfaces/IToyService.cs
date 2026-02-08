using RePlay.Domain.Entities;
using RePlay.Domain.Enums;

namespace RePlay.Application.Interfaces;

public interface IToyService
{
    Task<PagedResult<ToyDto>> GetToysAsync(ToyQueryParameters parameters);
    Task<ToyDto?> GetToyByIdAsync(Guid id);
    Task<ToyDto?> GetToyBySlugAsync(string slug);
    Task<ToyDto> CreateToyAsync(CreateToyDto dto, Guid adminId);
    Task<ToyDto?> UpdateToyAsync(Guid id, UpdateToyDto dto);
    Task<bool> ArchiveToyAsync(Guid id);
    Task<bool> RestoreToyAsync(Guid id);
    Task<string> AddToyImageAsync(Guid toyId, string imagePath, int displayOrder);
    Task<bool> RemoveToyImageAsync(Guid toyId, Guid imageId);
    Task<List<ToyDto>> GetToysByHolderAsync(Guid userId);
}

public class ToyQueryParameters
{
    public string? SearchTerm { get; set; }
    public ToyCategory? Category { get; set; }
    public ToyCondition? MinCondition { get; set; }
    public string? AgeGroup { get; set; }
    public ToyStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "newest";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool IncludeArchived { get; set; } = false;
}

public class ToyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public int Condition { get; set; }
    public string ConditionLabel { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public string? ShareableSlug { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ToyImageDto> Images { get; set; } = new();
}

public class ToyImageDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class CreateToyDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ToyCategory Category { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public ToyCondition Condition { get; set; }
    public decimal Price { get; set; }
}

public class UpdateToyDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ToyCategory? Category { get; set; }
    public string? AgeGroup { get; set; }
    public ToyCondition? Condition { get; set; }
    public decimal? Price { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
