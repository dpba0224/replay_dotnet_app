using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;

namespace RePlay.Infrastructure.Data.Configurations;

public class ToyConfiguration : IEntityTypeConfiguration<Toy>
{
    public void Configure(EntityTypeBuilder<Toy> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.AgeGroup)
            .HasMaxLength(20);

        builder.Property(t => t.Condition)
            .HasConversion<int>();

        builder.Property(t => t.Price)
            .HasPrecision(10, 2);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ToyStatus.Available);

        builder.Property(t => t.IsArchived)
            .HasDefaultValue(false);

        builder.Property(t => t.ShareableSlug)
            .HasMaxLength(50);

        builder.HasIndex(t => t.ShareableSlug)
            .IsUnique()
            .HasFilter("\"ShareableSlug\" IS NOT NULL");

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Category);
        builder.HasIndex(t => t.IsArchived);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(t => t.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(t => t.CreatedByAdmin)
            .WithMany(u => u.ToysCreated)
            .HasForeignKey(t => t.CreatedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CurrentHolder)
            .WithMany(u => u.ToysHeld)
            .HasForeignKey(t => t.CurrentHolderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
