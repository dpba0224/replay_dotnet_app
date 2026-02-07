namespace RePlay.Domain.Entities;

public class Rating
{
    public Guid Id { get; set; }
    public Guid RatedUserId { get; set; }
    public Guid RatedByAdminId { get; set; }
    public Guid ToyReturnId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User RatedUser { get; set; } = null!;
    public virtual User RatedByAdmin { get; set; } = null!;
    public virtual ToyReturn ToyReturn { get; set; } = null!;
}
