using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RePlay.Domain.Entities;

namespace RePlay.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Score)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(r => r.RatedUser)
            .WithMany(u => u.RatingsReceived)
            .HasForeignKey(r => r.RatedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RatedByAdmin)
            .WithMany(u => u.RatingsGiven)
            .HasForeignKey(r => r.RatedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ToyReturn)
            .WithOne(tr => tr.Rating)
            .HasForeignKey<Rating>(r => r.ToyReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
