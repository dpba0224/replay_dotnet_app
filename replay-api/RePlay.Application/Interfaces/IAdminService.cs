namespace RePlay.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<PagedResult<UserDto>> GetUsersAsync(UserQueryParameters parameters);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionQueryParameters parameters);
}

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalToys { get; set; }
    public int AvailableToys { get; set; }
    public int PendingTrades { get; set; }
    public int CompletedTrades { get; set; }
    public int PendingReturns { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class UserQueryParameters
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? Role { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TransactionQueryParameters
{
    public Guid? UserId { get; set; }
    public string? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TransactionDto
{
    public Guid Id { get; set; }
    public UserDto User { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public ToyDto Toy { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal? AmountPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}
