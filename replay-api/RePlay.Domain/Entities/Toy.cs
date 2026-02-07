using RePlay.Domain.Enums;

namespace RePlay.Domain.Entities;

public class Toy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ToyCategory Category { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public ToyCondition Condition { get; set; }
    public decimal Price { get; set; }
    public ToyStatus Status { get; set; } = ToyStatus.Available;
    public bool IsArchived { get; set; } = false;
    public string? ShareableSlug { get; set; }
    public Guid CreatedByAdminId { get; set; }
    public Guid? CurrentHolderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User CreatedByAdmin { get; set; } = null!;
    public virtual User? CurrentHolder { get; set; }
    public virtual ICollection<ToyImage> Images { get; set; } = new List<ToyImage>();
    public virtual ICollection<Trade> TradesAsRequested { get; set; } = new List<Trade>();
    public virtual ICollection<Trade> TradesAsOffered { get; set; } = new List<Trade>();
    public virtual ICollection<ToyReturn> Returns { get; set; } = new List<ToyReturn>();
    public virtual ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
}
