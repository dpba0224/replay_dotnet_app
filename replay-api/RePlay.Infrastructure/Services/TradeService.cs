using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class TradeService : ITradeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TradeService> _logger;

    public TradeService(AppDbContext context, ILogger<TradeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TradeResult> CreateTradeAsync(CreateTradeDto dto, Guid userId)
    {
        // Validate requested toy exists and is available
        var requestedToy = await _context.Toys
            .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == dto.RequestedToyId);

        if (requestedToy == null)
            return TradeResult.Failure("Requested toy not found.");

        if (requestedToy.IsArchived)
            return TradeResult.Failure("This toy is no longer available.");

        if (requestedToy.Status != ToyStatus.Available)
            return TradeResult.Failure($"This toy is not available for trading. Current status: {requestedToy.Status}.");

        // Validate user is not the current holder
        if (requestedToy.CurrentHolderId == userId)
            return TradeResult.Failure("You already have this toy.");

        // Check for existing pending trade by this user for the same toy
        var existingTrade = await _context.Trades
            .AnyAsync(t => t.UserId == userId
                && t.RequestedToyId == dto.RequestedToyId
                && t.Status == TradeStatus.Pending);

        if (existingTrade)
            return TradeResult.Failure("You already have a pending trade request for this toy.");

        Toy? offeredToy = null;

        if (dto.TradeType == TradeType.Trade)
        {
            // Validate offered toy for trade
            if (!dto.OfferedToyId.HasValue)
                return TradeResult.Failure("You must offer a toy when creating a trade request.");

            offeredToy = await _context.Toys
                .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(t => t.Id == dto.OfferedToyId.Value);

            if (offeredToy == null)
                return TradeResult.Failure("Offered toy not found.");

            // Verify the user currently holds the offered toy
            if (offeredToy.CurrentHolderId != userId)
                return TradeResult.Failure("You can only offer toys that you currently hold.");

            if (offeredToy.Status != ToyStatus.Traded)
                return TradeResult.Failure("The offered toy is not in a tradeable state.");
        }
        else if (dto.TradeType == TradeType.Purchase)
        {
            // For purchases, OfferedToyId should be null
            if (dto.OfferedToyId.HasValue)
                return TradeResult.Failure("You cannot offer a toy when making a purchase. Use the trade option instead.");
        }

        // Create the trade record
        var trade = new Trade
        {
            Id = Guid.NewGuid(),
            RequestedToyId = dto.RequestedToyId,
            OfferedToyId = dto.OfferedToyId,
            UserId = userId,
            TradeType = dto.TradeType,
            Status = TradeStatus.Pending,
            Notes = dto.Notes,
            AmountPaid = dto.TradeType == TradeType.Purchase ? requestedToy.Price : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.Trades.Add(trade);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Trade request created: {TradeId} by user {UserId} for toy {ToyId} (Type: {TradeType})",
            trade.Id, userId, dto.RequestedToyId, dto.TradeType);

        // Reload with navigation properties for the response
        var createdTrade = await GetTradeEntityAsync(trade.Id);
        var tradeDto = MapToDto(createdTrade!);

        return TradeResult.Success(tradeDto, "Trade request created successfully.");
    }

    public async Task<PagedResult<TradeDto>> GetUserTradesAsync(Guid userId, TradeQueryParameters parameters)
    {
        var query = _context.Trades
            .Include(t => t.RequestedToy).ThenInclude(t => t.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.OfferedToy).ThenInclude(t => t!.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        query = ApplyFilters(query, parameters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<TradeDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<TradeDto>> GetAllTradesAsync(TradeQueryParameters parameters)
    {
        var query = _context.Trades
            .Include(t => t.RequestedToy).ThenInclude(t => t.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.OfferedToy).ThenInclude(t => t!.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.User)
            .AsQueryable();

        query = ApplyFilters(query, parameters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<TradeDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TradeDto?> GetTradeByIdAsync(Guid id, Guid userId)
    {
        var trade = await _context.Trades
            .Include(t => t.RequestedToy).ThenInclude(t => t.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.OfferedToy).ThenInclude(t => t!.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trade == null)
            return null;

        return MapToDto(trade);
    }

    public async Task<TradeResult> ApproveTradeAsync(Guid tradeId, Guid adminId)
    {
        var trade = await GetTradeEntityAsync(tradeId);

        if (trade == null)
            return TradeResult.Failure("Trade not found.");

        if (trade.Status != TradeStatus.Pending)
            return TradeResult.Failure($"Trade cannot be approved. Current status: {trade.Status}.");

        // Re-check toy availability at approval time
        var requestedToy = await _context.Toys.FindAsync(trade.RequestedToyId);
        if (requestedToy == null || requestedToy.Status != ToyStatus.Available)
            return TradeResult.Failure("The requested toy is no longer available.");

        // For purchase type, verify payment was made (Stripe webhook will fill this in later)
        if (trade.TradeType == TradeType.Purchase && string.IsNullOrEmpty(trade.StripePaymentIntentId))
        {
            _logger.LogWarning("Approving purchase trade {TradeId} without Stripe payment confirmation.", tradeId);
        }

        // Update trade status
        trade.Status = TradeStatus.Approved;
        trade.CompletedAt = DateTime.UtcNow;

        // Update toy status and holder
        if (trade.TradeType == TradeType.Trade)
        {
            requestedToy.Status = ToyStatus.Traded;
            requestedToy.CurrentHolderId = trade.UserId;
            requestedToy.UpdatedAt = DateTime.UtcNow;

            // Return the offered toy to the platform inventory
            if (trade.OfferedToyId.HasValue)
            {
                var offeredToy = await _context.Toys.FindAsync(trade.OfferedToyId.Value);
                if (offeredToy != null)
                {
                    offeredToy.Status = ToyStatus.Available;
                    offeredToy.CurrentHolderId = null;
                    offeredToy.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        else if (trade.TradeType == TradeType.Purchase)
        {
            requestedToy.Status = ToyStatus.Sold;
            requestedToy.CurrentHolderId = trade.UserId;
            requestedToy.UpdatedAt = DateTime.UtcNow;
        }

        // Record transaction history
        var transaction = new TransactionHistory
        {
            Id = Guid.NewGuid(),
            UserId = trade.UserId,
            Type = trade.TradeType == TradeType.Trade ? TransactionType.Trade : TransactionType.Purchase,
            ToyId = trade.RequestedToyId,
            RelatedTradeId = trade.Id,
            Description = trade.TradeType == TradeType.Trade
                ? $"Traded for {requestedToy.Name}"
                : $"Purchased {requestedToy.Name} for ${trade.AmountPaid:F2}",
            AmountPaid = trade.AmountPaid,
            CreatedAt = DateTime.UtcNow
        };

        _context.TransactionHistories.Add(transaction);

        // Update user's trade count
        var user = await _context.Users.FindAsync(trade.UserId);
        if (user != null)
        {
            user.TotalTradesCompleted++;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Trade {TradeId} approved by admin {AdminId}. Toy {ToyId} assigned to user {UserId}.",
            tradeId, adminId, trade.RequestedToyId, trade.UserId);

        // Reload for response
        var approvedTrade = await GetTradeEntityAsync(tradeId);
        return TradeResult.Success(MapToDto(approvedTrade!), "Trade approved successfully.");
    }

    public async Task<TradeResult> CancelTradeAsync(Guid tradeId, Guid userId)
    {
        var trade = await GetTradeEntityAsync(tradeId);

        if (trade == null)
            return TradeResult.Failure("Trade not found.");

        // Only the trade creator can cancel
        if (trade.UserId != userId)
            return TradeResult.Failure("You can only cancel your own trade requests.");

        if (trade.Status != TradeStatus.Pending)
            return TradeResult.Failure($"Trade cannot be cancelled. Current status: {trade.Status}.");

        trade.Status = TradeStatus.Cancelled;
        trade.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Trade {TradeId} cancelled by user {UserId}.", tradeId, userId);

        return TradeResult.Success(MapToDto(trade), "Trade cancelled successfully.");
    }

    private async Task<Trade?> GetTradeEntityAsync(Guid tradeId)
    {
        return await _context.Trades
            .Include(t => t.RequestedToy).ThenInclude(t => t.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.OfferedToy).ThenInclude(t => t!.Images.OrderBy(i => i.DisplayOrder))
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tradeId);
    }

    private static IQueryable<Trade> ApplyFilters(IQueryable<Trade> query, TradeQueryParameters parameters)
    {
        if (parameters.Status.HasValue)
            query = query.Where(t => t.Status == parameters.Status.Value);

        if (parameters.Type.HasValue)
            query = query.Where(t => t.TradeType == parameters.Type.Value);

        if (parameters.FromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(t => t.CreatedAt <= parameters.ToDate.Value);

        return query;
    }

    private static TradeDto MapToDto(Trade trade)
    {
        return new TradeDto
        {
            Id = trade.Id,
            RequestedToy = MapToyToDto(trade.RequestedToy),
            OfferedToy = trade.OfferedToy != null ? MapToyToDto(trade.OfferedToy) : null,
            User = new UserDto
            {
                Id = trade.User.Id,
                Email = trade.User.Email ?? string.Empty,
                FullName = trade.User.FullName,
                ReputationScore = trade.User.ReputationScore,
                TotalTradesCompleted = trade.User.TotalTradesCompleted,
                IsActive = trade.User.IsActive,
                CreatedAt = trade.User.CreatedAt
            },
            TradeType = trade.TradeType.ToString(),
            Status = trade.Status.ToString(),
            AmountPaid = trade.AmountPaid,
            Notes = trade.Notes,
            CreatedAt = trade.CreatedAt,
            CompletedAt = trade.CompletedAt
        };
    }

    private static ToyDto MapToyToDto(Toy toy)
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
}
