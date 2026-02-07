using RePlay.Domain.Enums;

namespace RePlay.Application.Interfaces;

public interface ITradeService
{
    Task<TradeResult> CreateTradeAsync(CreateTradeDto dto, Guid userId);
    Task<PagedResult<TradeDto>> GetUserTradesAsync(Guid userId, TradeQueryParameters parameters);
    Task<PagedResult<TradeDto>> GetAllTradesAsync(TradeQueryParameters parameters);
    Task<TradeDto?> GetTradeByIdAsync(Guid id, Guid userId);
    Task<TradeResult> ApproveTradeAsync(Guid tradeId, Guid adminId);
    Task<TradeResult> CancelTradeAsync(Guid tradeId, Guid userId);
}

public class CreateTradeDto
{
    public Guid RequestedToyId { get; set; }
    public Guid? OfferedToyId { get; set; }
    public TradeType TradeType { get; set; }
    public string? Notes { get; set; }
}

public class TradeQueryParameters
{
    public TradeStatus? Status { get; set; }
    public TradeType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TradeDto
{
    public Guid Id { get; set; }
    public ToyDto RequestedToy { get; set; } = null!;
    public ToyDto? OfferedToy { get; set; }
    public UserDto User { get; set; } = null!;
    public string TradeType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? AmountPaid { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TradeResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public TradeDto? Trade { get; set; }
    public string? StripeCheckoutUrl { get; set; }

    public static TradeResult Success(TradeDto trade, string? message = null, string? checkoutUrl = null)
        => new() { Succeeded = true, Trade = trade, Message = message, StripeCheckoutUrl = checkoutUrl };

    public static TradeResult Failure(string message)
        => new() { Succeeded = false, Message = message };
}
