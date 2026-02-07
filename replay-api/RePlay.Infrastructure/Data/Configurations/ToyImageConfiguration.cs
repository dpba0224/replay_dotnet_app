using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RePlay.Domain.Entities;

namespace RePlay.Infrastructure.Data.Configurations;

public class ToyImageConfiguration : IEntityTypeConfiguration<ToyImage>
{
    public void Configure(EntityTypeBuilder<ToyImage> builder)
    {
        builder.HasKey(ti => ti.Id);

        builder.Property(ti => ti.ImagePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(ti => ti.DisplayOrder)
            .HasDefaultValue(1);

        builder.Property(ti => ti.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationship
        builder.HasOne(ti => ti.Toy)
            .WithMany(t => t.Images)
            .HasForeignKey(ti => ti.ToyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
