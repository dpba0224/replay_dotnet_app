using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AppDbContext context, UserManager<User> userManager, ILogger<AdminService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(TransactionQueryParameters parameters)
    {
        var query = BuildTransactionQuery(parameters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<TransactionDto>
        {
            Items = items.Select(MapToTransactionDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<TransactionDto>> GetUserTransactionsAsync(Guid userId, TransactionQueryParameters parameters)
    {
        parameters.UserId = userId;
        var query = BuildTransactionQuery(parameters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<TransactionDto>
        {
            Items = items.Select(MapToTransactionDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsActive);
        var totalToys = await _context.Toys.CountAsync(t => !t.IsArchived);
        var availableToys = await _context.Toys.CountAsync(t => t.Status == ToyStatus.Available && !t.IsArchived);
        var pendingTrades = await _context.Trades.CountAsync(t => t.Status == TradeStatus.Pending);
        var completedTrades = await _context.Trades.CountAsync(t => t.Status == TradeStatus.Completed);
        var pendingReturns = await _context.ToyReturns.CountAsync(r => r.Status == ReturnStatus.Pending);
        var totalRevenue = await _context.TransactionHistories
            .Where(t => t.Type == TransactionType.Purchase && t.AmountPaid.HasValue)
            .SumAsync(t => t.AmountPaid ?? 0);

        var recentActivities = await _context.TransactionHistories
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new RecentActivityDto
            {
                Type = t.Type.ToString(),
                Description = t.Description,
                Timestamp = t.CreatedAt
            })
            .ToListAsync();

        return new DashboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalToys = totalToys,
            AvailableToys = availableToys,
            PendingTrades = pendingTrades,
            CompletedTrades = completedTrades,
            PendingReturns = pendingReturns,
            TotalRevenue = totalRevenue,
            RecentActivities = recentActivities
        };
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(UserQueryParameters parameters)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var term = parameters.SearchTerm.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email!.ToLower().Contains(term));
        }

        if (parameters.IsActive.HasValue)
            query = query.Where(u => u.IsActive == parameters.IsActive.Value);

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "User",
                ProfileImageUrl = user.ProfileImageUrl,
                ReputationScore = user.ReputationScore,
                TotalTradesCompleted = user.TotalTradesCompleted,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        return new PagedResult<UserDto>
        {
            Items = userDtos,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} activated.", userId);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} deactivated.", userId);
        return true;
    }

    private IQueryable<TransactionHistory> BuildTransactionQuery(TransactionQueryParameters parameters)
    {
        var query = _context.TransactionHistories
            .Include(t => t.User)
            .Include(t => t.Toy).ThenInclude(t => t.Images.OrderBy(i => i.DisplayOrder))
            .AsQueryable();

        if (parameters.UserId.HasValue)
            query = query.Where(t => t.UserId == parameters.UserId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.Type))
        {
            if (Enum.TryParse<TransactionType>(parameters.Type, true, out var transactionType))
                query = query.Where(t => t.Type == transactionType);
        }

        if (parameters.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= parameters.ToDate.Value);

        return query;
    }

    private static TransactionDto MapToTransactionDto(TransactionHistory transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            User = new UserDto
            {
                Id = transaction.User.Id,
                Email = transaction.User.Email ?? string.Empty,
                FullName = transaction.User.FullName,
                ReputationScore = transaction.User.ReputationScore,
                TotalTradesCompleted = transaction.User.TotalTradesCompleted,
                IsActive = transaction.User.IsActive,
                CreatedAt = transaction.User.CreatedAt
            },
            Type = transaction.Type.ToString(),
            Toy = new ToyDto
            {
                Id = transaction.Toy.Id,
                Name = transaction.Toy.Name,
                Description = transaction.Toy.Description,
                Category = transaction.Toy.Category.ToString(),
                AgeGroup = transaction.Toy.AgeGroup,
                Condition = (int)transaction.Toy.Condition,
                ConditionLabel = transaction.Toy.Condition.ToString(),
                Price = transaction.Toy.Price,
                Status = transaction.Toy.Status.ToString(),
                IsArchived = transaction.Toy.IsArchived,
                ShareableSlug = transaction.Toy.ShareableSlug,
                CreatedAt = transaction.Toy.CreatedAt,
                Images = transaction.Toy.Images.Select(i => new ToyImageDto
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
            },
            Description = transaction.Description,
            AmountPaid = transaction.AmountPaid,
            CreatedAt = transaction.CreatedAt
        };
    }
}
