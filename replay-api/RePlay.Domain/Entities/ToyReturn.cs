using RePlay.Domain.Enums;

namespace RePlay.Domain.Entities;

public class ToyReturn
{
    public Guid Id { get; set; }
    public Guid ToyId { get; set; }
    public Guid ReturnedByUserId { get; set; }
    public Guid? ApprovedByAdminId { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public ToyCondition? ConditionOnReturn { get; set; }
    public string? UserNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual Toy Toy { get; set; } = null!;
    public virtual User ReturnedByUser { get; set; } = null!;
    public virtual User? ApprovedByAdmin { get; set; }
    public virtual Rating? Rating { get; set; }
}
