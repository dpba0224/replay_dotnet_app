using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;

namespace RePlay.Infrastructure.Data.Configurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TradeType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(TradeStatus.Pending);

        builder.Property(t => t.StripePaymentIntentId)
            .HasMaxLength(255);

        builder.Property(t => t.AmountPaid)
            .HasPrecision(10, 2);

        builder.Property(t => t.Notes)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(t => t.Status);

        // Relationships
        builder.HasOne(t => t.RequestedToy)
            .WithMany(toy => toy.TradesAsRequested)
            .HasForeignKey(t => t.RequestedToyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.OfferedToy)
            .WithMany(toy => toy.TradesAsOffered)
            .HasForeignKey(t => t.OfferedToyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.User)
            .WithMany(u => u.Trades)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
