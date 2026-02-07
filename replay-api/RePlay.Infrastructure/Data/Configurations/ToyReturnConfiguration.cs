using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;

namespace RePlay.Infrastructure.Data.Configurations;

public class ToyReturnConfiguration : IEntityTypeConfiguration<ToyReturn>
{
    public void Configure(EntityTypeBuilder<ToyReturn> builder)
    {
        builder.HasKey(tr => tr.Id);

        builder.Property(tr => tr.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ReturnStatus.Pending);

        builder.Property(tr => tr.ConditionOnReturn)
            .HasConversion<int?>();

        builder.Property(tr => tr.UserNotes)
            .HasMaxLength(500);

        builder.Property(tr => tr.AdminNotes)
            .HasMaxLength(500);

        builder.Property(tr => tr.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(tr => tr.Status);

        // Relationships
        builder.HasOne(tr => tr.Toy)
            .WithMany(t => t.Returns)
            .HasForeignKey(tr => tr.ToyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.ReturnedByUser)
            .WithMany(u => u.ToyReturns)
            .HasForeignKey(tr => tr.ReturnedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(tr => tr.ApprovedByAdmin)
            .WithMany()
            .HasForeignKey(tr => tr.ApprovedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
