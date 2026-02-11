using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RePlay.Application.Interfaces;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;

namespace RePlay.Infrastructure.Services;

public class ReturnService : IReturnService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReturnService> _logger;

    public ReturnService(AppDbContext context, ILogger<ReturnService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReturnResult> InitiateReturnAsync(CreateReturnDto dto, Guid userId)
    {
        // Validate toy exists
        var toy = await _context.Toys
            .Include(t => t.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == dto.ToyId);

        if (toy == null)
            return ReturnResult.Failure("Toy not found.");

        // Verify the user currently holds this toy
        if (toy.CurrentHolderId != userId)
            return ReturnResult.Failure("You can only return toys that you currently hold.");

        // Check toy is in a returnable state (Traded or Sold)
        if (toy.Status != ToyStatus.Traded && toy.Status != ToyStatus.Sold)
            return ReturnResult.Failure($"This toy cannot be returned. Current status: {toy.Status}.");

        // Check for existing pending return
        var existingReturn = await _context.ToyReturns
            .AnyAsync(r => r.ToyId == dto.ToyId
                && r.ReturnedByUserId == userId
                && r.Status == ReturnStatus.Pending);

        if (existingReturn)
            return ReturnResult.Failure("You already have a pending return request for this toy.");

        // Create the return record
        var toyReturn = new ToyReturn
        {
            Id = Guid.NewGuid(),
            ToyId = dto.ToyId,
            ReturnedByUserId = userId,
            Status = ReturnStatus.Pending,
            UserNotes = dto.UserNotes,
            CreatedAt = DateTime.UtcNow
        };

        _context.ToyReturns.Add(toyReturn);

        // Update toy status to PendingReturn
        toy.Status = ToyStatus.PendingReturn;
        toy.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Return initiated: {ReturnId} by user {UserId} for toy {ToyId}",
            toyReturn.Id, userId, dto.ToyId);

        // Reload with navigation properties
        var createdReturn = await GetReturnEntityAsync(toyReturn.Id);
        return ReturnResult.Success(MapToDto(createdReturn!), "Return request submitted successfully.");
    }

    public async Task<PagedResult<ReturnDto>> GetUserReturnsAsync(Guid userId, ReturnQueryParameters parameters)
    {
        var query = BuildQuery()
            .Where(r => r.ReturnedByUserId == userId);

        query = ApplyFilters(query, parameters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ReturnDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ReturnDto>> GetAllReturnsAsync(ReturnQueryParameters parameters)
    {
        var query = BuildQuery();

        query = ApplyFilters(query, parameters);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<ReturnDto>
        {
            Items = items.Select(MapToDto).ToList(),
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ReturnResult> ApproveReturnAsync(Guid returnId, ApproveReturnDto dto, Guid adminId)
    {
        var toyReturn = await GetReturnEntityAsync(returnId);

        if (toyReturn == null)
            return ReturnResult.Failure("Return not found.");

        if (toyReturn.Status != ReturnStatus.Pending)
            return ReturnResult.Failure($"Return cannot be approved. Current status: {toyReturn.Status}.");

        // Update return record
        toyReturn.Status = ReturnStatus.Approved;
        toyReturn.ApprovedByAdminId = adminId;
        toyReturn.ConditionOnReturn = dto.ConditionOnReturn;
        toyReturn.AdminNotes = dto.AdminNotes;
        toyReturn.ResolvedAt = DateTime.UtcNow;

        // Update toy: set back to Available, update condition, clear holder
        var toy = await _context.Toys.FindAsync(toyReturn.ToyId);
        if (toy != null)
        {
            toy.Status = ToyStatus.Available;
            toy.Condition = dto.ConditionOnReturn;
            toy.CurrentHolderId = null;
            toy.UpdatedAt = DateTime.UtcNow;
        }

        // Create user rating if provided
        if (dto.UserRating.HasValue && dto.UserRating.Value >= 1 && dto.UserRating.Value <= 5)
        {
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                RatedUserId = toyReturn.ReturnedByUserId,
                RatedByAdminId = adminId,
                ToyReturnId = returnId,
                Score = dto.UserRating.Value,
                Comment = dto.RatingComment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Ratings.Add(rating);

            // Recalculate user's reputation score
            await UpdateUserReputationAsync(toyReturn.ReturnedByUserId, dto.UserRating.Value);
        }

        // Record transaction history
        var transaction = new TransactionHistory
        {
            Id = Guid.NewGuid(),
            UserId = toyReturn.ReturnedByUserId,
            Type = TransactionType.ReturnApproved,
            ToyId = toyReturn.ToyId,
            Description = $"Return approved for {toy?.Name ?? "toy"}. Condition: {dto.ConditionOnReturn}",
            CreatedAt = DateTime.UtcNow
        };

        _context.TransactionHistories.Add(transaction);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Return {ReturnId} approved by admin {AdminId}. Toy {ToyId} back to Available with condition {Condition}.",
            returnId, adminId, toyReturn.ToyId, dto.ConditionOnReturn);

        // Reload for response
        var approvedReturn = await GetReturnEntityAsync(returnId);
        return ReturnResult.Success(MapToDto(approvedReturn!), "Return approved successfully.");
    }

    public async Task<ReturnResult> RejectReturnAsync(Guid returnId, string adminNotes, Guid adminId)
    {
        var toyReturn = await GetReturnEntityAsync(returnId);

        if (toyReturn == null)
            return ReturnResult.Failure("Return not found.");

        if (toyReturn.Status != ReturnStatus.Pending)
            return ReturnResult.Failure($"Return cannot be rejected. Current status: {toyReturn.Status}.");

        // Update return record
        toyReturn.Status = ReturnStatus.Rejected;
        toyReturn.ApprovedByAdminId = adminId;
        toyReturn.AdminNotes = adminNotes;
        toyReturn.ResolvedAt = DateTime.UtcNow;

        // Revert toy status back from PendingReturn
        var toy = await _context.Toys.FindAsync(toyReturn.ToyId);
        if (toy != null && toy.Status == ToyStatus.PendingReturn)
        {
            // Determine previous status from the last completed trade for this toy
            var lastTrade = await _context.Trades
                .Where(t => t.RequestedToyId == toy.Id && t.UserId == toyReturn.ReturnedByUserId
                    && (t.Status == TradeStatus.Approved || t.Status == TradeStatus.Completed))
                .OrderByDescending(t => t.CompletedAt)
                .FirstOrDefaultAsync();

            toy.Status = lastTrade?.TradeType == TradeType.Purchase ? ToyStatus.Sold : ToyStatus.Traded;
            toy.UpdatedAt = DateTime.UtcNow;
        }

        // Record transaction history
        var transaction = new TransactionHistory
        {
            Id = Guid.NewGuid(),
            UserId = toyReturn.ReturnedByUserId,
            Type = TransactionType.ReturnRejected,
            ToyId = toyReturn.ToyId,
            Description = $"Return rejected for {toy?.Name ?? "toy"}. Reason: {adminNotes}",
            CreatedAt = DateTime.UtcNow
        };

        _context.TransactionHistories.Add(transaction);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Return {ReturnId} rejected by admin {AdminId}. Toy {ToyId} remains with user.",
            returnId, adminId, toyReturn.ToyId);

        var rejectedReturn = await GetReturnEntityAsync(returnId);
        return ReturnResult.Success(MapToDto(rejectedReturn!), "Return rejected.");
    }

    private async Task UpdateUserReputationAsync(Guid userId, int newScore)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        // Get all previously saved ratings for this user
        var allScores = await _context.Ratings
            .Where(r => r.RatedUserId == userId)
            .Select(r => r.Score)
            .ToListAsync();

        // Include the new rating score (not yet saved to database)
        allScores.Add(newScore);

        user.ReputationScore = allScores.Count > 0
            ? Math.Round((decimal)allScores.Average(), 2)
            : 0;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private IQueryable<ToyReturn> BuildQuery()
    {
        return _context.ToyReturns
            .Include(r => r.Toy).ThenInclude(t => t.Images.OrderBy(i => i.DisplayOrder))
            .Include(r => r.ReturnedByUser)
            .Include(r => r.ApprovedByAdmin)
            .AsQueryable();
    }

    private async Task<ToyReturn?> GetReturnEntityAsync(Guid returnId)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(r => r.Id == returnId);
    }

    private static IQueryable<ToyReturn> ApplyFilters(IQueryable<ToyReturn> query, ReturnQueryParameters parameters)
    {
        if (parameters.Status.HasValue)
            query = query.Where(r => r.Status == parameters.Status.Value);

        if (parameters.FromDate.HasValue)
            query = query.Where(r => r.CreatedAt >= parameters.FromDate.Value);

        if (parameters.ToDate.HasValue)
            query = query.Where(r => r.CreatedAt <= parameters.ToDate.Value);

        return query;
    }

    private static ReturnDto MapToDto(ToyReturn toyReturn)
    {
        return new ReturnDto
        {
            Id = toyReturn.Id,
            Toy = new ToyDto
            {
                Id = toyReturn.Toy.Id,
                Name = toyReturn.Toy.Name,
                Description = toyReturn.Toy.Description,
                Category = toyReturn.Toy.Category.ToString(),
                AgeGroup = toyReturn.Toy.AgeGroup,
                Condition = (int)toyReturn.Toy.Condition,
                ConditionLabel = toyReturn.Toy.Condition.ToString(),
                Price = toyReturn.Toy.Price,
                Status = toyReturn.Toy.Status.ToString(),
                IsArchived = toyReturn.Toy.IsArchived,
                ShareableSlug = toyReturn.Toy.ShareableSlug,
                CreatedAt = toyReturn.Toy.CreatedAt,
                Images = toyReturn.Toy.Images.Select(i => new ToyImageDto
                {
                    Id = i.Id,
                    ImagePath = i.ImagePath,
                    DisplayOrder = i.DisplayOrder
                }).ToList()
            },
            ReturnedByUser = new UserDto
            {
                Id = toyReturn.ReturnedByUser.Id,
                Email = toyReturn.ReturnedByUser.Email ?? string.Empty,
                FullName = toyReturn.ReturnedByUser.FullName,
                ReputationScore = toyReturn.ReturnedByUser.ReputationScore,
                TotalTradesCompleted = toyReturn.ReturnedByUser.TotalTradesCompleted,
                IsActive = toyReturn.ReturnedByUser.IsActive,
                CreatedAt = toyReturn.ReturnedByUser.CreatedAt
            },
            ApprovedByAdmin = toyReturn.ApprovedByAdmin != null ? new UserDto
            {
                Id = toyReturn.ApprovedByAdmin.Id,
                Email = toyReturn.ApprovedByAdmin.Email ?? string.Empty,
                FullName = toyReturn.ApprovedByAdmin.FullName,
                CreatedAt = toyReturn.ApprovedByAdmin.CreatedAt
            } : null,
            Status = toyReturn.Status.ToString(),
            ConditionOnReturn = toyReturn.ConditionOnReturn.HasValue ? (int)toyReturn.ConditionOnReturn.Value : null,
            UserNotes = toyReturn.UserNotes,
            AdminNotes = toyReturn.AdminNotes,
            CreatedAt = toyReturn.CreatedAt,
            ResolvedAt = toyReturn.ResolvedAt
        };
    }
}
