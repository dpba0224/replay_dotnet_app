using Microsoft.AspNetCore.Identity;

namespace RePlay.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? ProfileImageUrl { get; set; }
    public decimal ReputationScore { get; set; } = 0;
    public int TotalTradesCompleted { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
    public virtual ICollection<ToyReturn> ToyReturns { get; set; } = new List<ToyReturn>();
    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    public virtual ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
    public virtual ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
    public virtual ICollection<TransactionHistory> TransactionHistories { get; set; } = new List<TransactionHistory>();
    public virtual ICollection<Toy> ToysCreated { get; set; } = new List<Toy>();
    public virtual ICollection<Toy> ToysHeld { get; set; } = new List<Toy>();
}
