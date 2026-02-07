namespace RePlay.Domain.Entities;

public class ToyImage
{
    public Guid Id { get; set; }
    public Guid ToyId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual Toy Toy { get; set; } = null!;
}
