using RePlay.Domain.Enums;

namespace RePlay.Domain.Entities;

public class TransactionHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public TransactionType Type { get; set; }
    public Guid ToyId { get; set; }
    public Guid? RelatedTradeId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal? AmountPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Toy Toy { get; set; } = null!;
    public virtual Trade? RelatedTrade { get; set; }
}
