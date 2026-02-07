using RePlay.Domain.Enums;

namespace RePlay.Domain.Entities;

public class Trade
{
    public Guid Id { get; set; }
    public Guid RequestedToyId { get; set; }
    public Guid? OfferedToyId { get; set; }
    public Guid UserId { get; set; }
    public TradeType TradeType { get; set; }
    public TradeStatus Status { get; set; } = TradeStatus.Pending;
    public string? StripePaymentIntentId { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual Toy RequestedToy { get; set; } = null!;
    public virtual Toy? OfferedToy { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
}
