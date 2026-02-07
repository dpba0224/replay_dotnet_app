using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RePlay.Domain.Entities;

namespace RePlay.Infrastructure.Data.Configurations;

public class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
{
    public void Configure(EntityTypeBuilder<TransactionHistory> builder)
    {
        builder.HasKey(th => th.Id);

        builder.Property(th => th.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(th => th.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(th => th.AmountPaid)
            .HasPrecision(10, 2);

        builder.Property(th => th.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(th => th.CreatedAt);

        // Relationships
        builder.HasOne(th => th.User)
            .WithMany(u => u.TransactionHistories)
            .HasForeignKey(th => th.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(th => th.Toy)
            .WithMany(t => t.TransactionHistories)
            .HasForeignKey(th => th.ToyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(th => th.RelatedTrade)
            .WithMany(t => t.TransactionHistories)
            .HasForeignKey(th => th.RelatedTradeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
