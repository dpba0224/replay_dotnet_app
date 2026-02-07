using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RePlay.Domain.Entities;
using RePlay.Domain.Enums;

namespace RePlay.Infrastructure.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<DbSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedSampleToysAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                _logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var adminEmail = "admin@replay.com";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                _logger.LogInformation("Created admin user: {Email}", adminEmail);
            }
            else
            {
                _logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private async Task SeedSampleToysAsync()
    {
        if (await _context.Toys.AnyAsync())
        {
            _logger.LogInformation("Toys already seeded, skipping...");
            return;
        }

        var adminUser = await _userManager.FindByEmailAsync("admin@replay.com");
        if (adminUser == null)
        {
            _logger.LogWarning("Admin user not found, cannot seed toys");
            return;
        }

        var toys = new List<Toy>
        {
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "LEGO Star Wars Millennium Falcon",
                Description = "Build the legendary Millennium Falcon with this detailed LEGO set. Includes Han Solo, Chewbacca, and Princess Leia minifigures. Perfect for Star Wars fans and LEGO collectors.",
                Category = ToyCategory.BuildingSets,
                AgeGroup = "9-12",
                Condition = ToyCondition.Excellent,
                Price = 89.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "lego-millennium-falcon",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Funko Pop! Marvel - Spider-Man",
                Description = "Classic Spider-Man Funko Pop vinyl figure. Standing 3.75 inches tall, this collectible captures the iconic web-slinger in his classic red and blue suit.",
                Category = ToyCategory.ActionFiguresAndCollectibles,
                AgeGroup = "13+",
                Condition = ToyCondition.Mint,
                Price = 14.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "funko-spiderman",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Monopoly Classic Edition",
                Description = "The classic property trading board game that has entertained families for generations. Buy, sell, and trade your way to victory!",
                Category = ToyCategory.BoardGamesAndPuzzles,
                AgeGroup = "6-8",
                Condition = ToyCondition.Good,
                Price = 24.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "monopoly-classic",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Barbie Dreamhouse Playset",
                Description = "Three-story Barbie Dreamhouse with 8 rooms, a working elevator, pool with slide, and over 70 accessories. The ultimate playset for Barbie fans!",
                Category = ToyCategory.DollsAndPlush,
                AgeGroup = "3-5",
                Condition = ToyCondition.Good,
                Price = 149.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "barbie-dreamhouse",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Hot Wheels 20-Car Gift Pack",
                Description = "Collection of 20 Hot Wheels die-cast cars in various styles and colors. Perfect for racing, collecting, or creative play.",
                Category = ToyCategory.VehiclesAndRC,
                AgeGroup = "3-5",
                Condition = ToyCondition.Excellent,
                Price = 21.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "hotwheels-20pack",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "STEM Robot Building Kit",
                Description = "Educational robotics kit with 12 different robot configurations. Learn coding, engineering, and problem-solving while having fun!",
                Category = ToyCategory.EducationalAndSTEM,
                AgeGroup = "9-12",
                Condition = ToyCondition.Mint,
                Price = 59.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "stem-robot-kit",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Nerf Elite Disruptor Blaster",
                Description = "Quick-draw Nerf blaster with rotating drum that holds up to 6 Elite darts. Slam fire action lets you unleash all 6 darts rapidly!",
                Category = ToyCategory.OutdoorAndSports,
                AgeGroup = "6-8",
                Condition = ToyCondition.Good,
                Price = 12.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "nerf-disruptor",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Vintage Nintendo Game Boy",
                Description = "Original Nintendo Game Boy from 1989 in working condition. A piece of gaming history! Includes Tetris cartridge.",
                Category = ToyCategory.VintageAndRetro,
                AgeGroup = "All Ages",
                Condition = ToyCondition.Fair,
                Price = 89.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "vintage-gameboy",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Squishmallows - Luna the Owl",
                Description = "Super soft and cuddly 12-inch Squishmallow plush owl. Perfect for snuggling, collecting, or decorating!",
                Category = ToyCategory.DollsAndPlush,
                AgeGroup = "3-5",
                Condition = ToyCondition.Mint,
                Price = 19.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "squishmallow-luna",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Toy
            {
                Id = Guid.NewGuid(),
                Name = "Settlers of Catan Board Game",
                Description = "Award-winning strategy board game where you collect resources, build settlements, and trade with other players. 3-4 players.",
                Category = ToyCategory.BoardGamesAndPuzzles,
                AgeGroup = "9-12",
                Condition = ToyCondition.Excellent,
                Price = 44.99m,
                Status = ToyStatus.Available,
                ShareableSlug = "settlers-of-catan",
                CreatedByAdminId = adminUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await _context.Toys.AddRangeAsync(toys);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} sample toys", toys.Count);
    }
}
