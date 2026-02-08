using Microsoft.EntityFrameworkCore;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;

namespace RePlay.Tests;

/// <summary>
/// Shared helper for creating InMemory AppDbContext instances and seeding test data.
/// </summary>
public static class TestDbHelper
{
    public static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static User CreateUser(string name = "Test User", string email = "test@example.com")
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = name,
            Email = email,
            UserName = email,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
            EmailConfirmed = true,
            IsActive = true,
            ReputationScore = 0,
            TotalTradesCompleted = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User CreateAdmin(string name = "Admin User", string email = "admin@example.com")
    {
        return CreateUser(name, email);
    }

    public static Toy CreateToy(
        Guid adminId,
        string name = "Test Toy",
        ToyStatus status = ToyStatus.Available,
        decimal price = 100m,
        ToyCondition condition = ToyCondition.Good,
        Guid? holderId = null)
    {
        return new Toy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            Category = ToyCategory.ActionFiguresAndCollectibles,
            AgeGroup = "6-8",
            Condition = condition,
            Price = price,
            Status = status,
            IsArchived = false,
            ShareableSlug = $"{name.ToLower().Replace(" ", "-")}-{Guid.NewGuid():N}"[..20],
            CreatedByAdminId = adminId,
            CurrentHolderId = holderId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
