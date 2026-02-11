using Microsoft.Extensions.Logging;
using Moq;
using RePlay.Application.Interfaces;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Services;

namespace RePlay.Tests;

public class TradeServiceTests
{
    private readonly Mock<ILogger<TradeService>> _loggerMock = new();
    private readonly Mock<IEmailService> _emailMock = new();

    private TradeService CreateService(string dbName)
    {
        var context = TestDbHelper.CreateContext(dbName);
        return new TradeService(context, _loggerMock.Object, _emailMock.Object);
    }

    // --- CreateTradeAsync (Purchase) ---

    [Fact]
    public async Task CreateTrade_Purchase_Succeeds()
    {
        var dbName = nameof(CreateTrade_Purchase_Succeeds);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "LEGO Set", ToyStatus.Available, 250m);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            TradeType = TradeType.Purchase
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Trade);
        Assert.Equal("Purchase", result.Trade!.TradeType);
        Assert.Equal("Pending", result.Trade.Status);
        Assert.Equal(250m, result.Trade.AmountPaid);
    }

    [Fact]
    public async Task CreateTrade_Purchase_WithOfferedToy_Fails()
    {
        var dbName = nameof(CreateTrade_Purchase_WithOfferedToy_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id);
        var offeredToy = TestDbHelper.CreateToy(admin.Id, "Offered Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.AddRange(toy, offeredToy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            OfferedToyId = offeredToy.Id,
            TradeType = TradeType.Purchase
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("cannot offer a toy when making a purchase", result.Message!);
    }

    // --- CreateTradeAsync (Trade) ---

    [Fact]
    public async Task CreateTrade_Trade_Succeeds()
    {
        var dbName = nameof(CreateTrade_Trade_Succeeds);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var requestedToy = TestDbHelper.CreateToy(admin.Id, "Requested Toy");
        var offeredToy = TestDbHelper.CreateToy(admin.Id, "Offered Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.AddRange(requestedToy, offeredToy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = requestedToy.Id,
            OfferedToyId = offeredToy.Id,
            TradeType = TradeType.Trade
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Trade);
        Assert.Equal("Trade", result.Trade!.TradeType);
        Assert.Null(result.Trade.AmountPaid);
    }

    [Fact]
    public async Task CreateTrade_Trade_WithoutOfferedToy_Fails()
    {
        var dbName = nameof(CreateTrade_Trade_WithoutOfferedToy_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            TradeType = TradeType.Trade
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("must offer a toy", result.Message!);
    }

    [Fact]
    public async Task CreateTrade_Trade_OfferedToyNotHeld_Fails()
    {
        var dbName = nameof(CreateTrade_Trade_OfferedToyNotHeld_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var otherUser = TestDbHelper.CreateUser("Other User", "other@example.com");
        var requestedToy = TestDbHelper.CreateToy(admin.Id, "Requested");
        var offeredToy = TestDbHelper.CreateToy(admin.Id, "Offered", ToyStatus.Traded, holderId: otherUser.Id);

        context.Users.AddRange(admin, user, otherUser);
        context.Toys.AddRange(requestedToy, offeredToy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = requestedToy.Id,
            OfferedToyId = offeredToy.Id,
            TradeType = TradeType.Trade
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("only offer toys that you currently hold", result.Message!);
    }

    // --- Validation Tests ---

    [Fact]
    public async Task CreateTrade_NonexistentToy_Fails()
    {
        var dbName = nameof(CreateTrade_NonexistentToy_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var user = TestDbHelper.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = Guid.NewGuid(),
            TradeType = TradeType.Purchase
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Message!);
    }

    [Fact]
    public async Task CreateTrade_ArchivedToy_Fails()
    {
        var dbName = nameof(CreateTrade_ArchivedToy_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id);
        toy.IsArchived = true;

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            TradeType = TradeType.Purchase
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("no longer available", result.Message!);
    }

    [Fact]
    public async Task CreateTrade_UnavailableToy_Fails()
    {
        var dbName = nameof(CreateTrade_UnavailableToy_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, status: ToyStatus.Sold);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            TradeType = TradeType.Purchase
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("not available for trading", result.Message!);
    }

    [Fact]
    public async Task CreateTrade_AlreadyHeldByUser_Fails()
    {
        var dbName = nameof(CreateTrade_AlreadyHeldByUser_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, status: ToyStatus.Available, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            TradeType = TradeType.Purchase
        };

        var result = await service.CreateTradeAsync(dto, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("already have this toy", result.Message!);
    }

    [Fact]
    public async Task CreateTrade_DuplicatePending_Fails()
    {
        var dbName = nameof(CreateTrade_DuplicatePending_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);
        var dto = new CreateTradeDto
        {
            RequestedToyId = toy.Id,
            TradeType = TradeType.Purchase
        };

        // First trade succeeds
        var result1 = await service.CreateTradeAsync(dto, user.Id);
        Assert.True(result1.Succeeded);

        // Second trade for same toy fails
        var result2 = await service.CreateTradeAsync(dto, user.Id);
        Assert.False(result2.Succeeded);
        Assert.Contains("already have a pending trade", result2.Message!);
    }

    // --- ApproveTradeAsync ---

    [Fact]
    public async Task ApproveTrade_Purchase_SetsToySold()
    {
        var dbName = nameof(ApproveTrade_Purchase_SetsToySold);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Purchasable Toy", ToyStatus.Available, 150m);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        // Create the trade first
        var createResult = await service.CreateTradeAsync(
            new CreateTradeDto { RequestedToyId = toy.Id, TradeType = TradeType.Purchase },
            user.Id);
        Assert.True(createResult.Succeeded);

        // Approve it
        var approveResult = await service.ApproveTradeAsync(createResult.Trade!.Id, admin.Id);

        Assert.True(approveResult.Succeeded);
        Assert.Equal("Approved", approveResult.Trade!.Status);

        // Verify toy state
        var updatedToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.Sold, updatedToy!.Status);
        Assert.Equal(user.Id, updatedToy.CurrentHolderId);

        // Verify user trade count incremented
        var updatedUser = await context.Users.FindAsync(user.Id);
        Assert.Equal(1, updatedUser!.TotalTradesCompleted);

        // Verify transaction history was recorded
        Assert.Single(context.TransactionHistories);
    }

    [Fact]
    public async Task ApproveTrade_Trade_SwapsToys()
    {
        var dbName = nameof(ApproveTrade_Trade_SwapsToys);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var requestedToy = TestDbHelper.CreateToy(admin.Id, "Requested Toy");
        var offeredToy = TestDbHelper.CreateToy(admin.Id, "Offered Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.AddRange(requestedToy, offeredToy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        var createResult = await service.CreateTradeAsync(
            new CreateTradeDto
            {
                RequestedToyId = requestedToy.Id,
                OfferedToyId = offeredToy.Id,
                TradeType = TradeType.Trade
            }, user.Id);

        var approveResult = await service.ApproveTradeAsync(createResult.Trade!.Id, admin.Id);

        Assert.True(approveResult.Succeeded);

        // Requested toy now held by user
        var updatedRequested = await context.Toys.FindAsync(requestedToy.Id);
        Assert.Equal(ToyStatus.Traded, updatedRequested!.Status);
        Assert.Equal(user.Id, updatedRequested.CurrentHolderId);

        // Offered toy returned to platform
        var updatedOffered = await context.Toys.FindAsync(offeredToy.Id);
        Assert.Equal(ToyStatus.Available, updatedOffered!.Status);
        Assert.Null(updatedOffered.CurrentHolderId);
    }

    [Fact]
    public async Task ApproveTrade_NotPending_Fails()
    {
        var dbName = nameof(ApproveTrade_NotPending_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        var createResult = await service.CreateTradeAsync(
            new CreateTradeDto { RequestedToyId = toy.Id, TradeType = TradeType.Purchase },
            user.Id);

        // Cancel the trade first
        await service.CancelTradeAsync(createResult.Trade!.Id, user.Id);

        // Try to approve cancelled trade
        var approveResult = await service.ApproveTradeAsync(createResult.Trade.Id, admin.Id);

        Assert.False(approveResult.Succeeded);
        Assert.Contains("cannot be approved", approveResult.Message!);
    }

    [Fact]
    public async Task ApproveTrade_NonexistentTrade_Fails()
    {
        var dbName = nameof(ApproveTrade_NonexistentTrade_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        var result = await service.ApproveTradeAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Message!);
    }

    // --- CancelTradeAsync ---

    [Fact]
    public async Task CancelTrade_ByOwner_Succeeds()
    {
        var dbName = nameof(CancelTrade_ByOwner_Succeeds);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        var createResult = await service.CreateTradeAsync(
            new CreateTradeDto { RequestedToyId = toy.Id, TradeType = TradeType.Purchase },
            user.Id);

        var cancelResult = await service.CancelTradeAsync(createResult.Trade!.Id, user.Id);

        Assert.True(cancelResult.Succeeded);
        Assert.Equal("Cancelled", cancelResult.Trade!.Status);
    }

    [Fact]
    public async Task CancelTrade_ByOtherUser_Fails()
    {
        var dbName = nameof(CancelTrade_ByOtherUser_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var otherUser = TestDbHelper.CreateUser("Other User", "other@example.com");
        var toy = TestDbHelper.CreateToy(admin.Id);

        context.Users.AddRange(admin, user, otherUser);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        var createResult = await service.CreateTradeAsync(
            new CreateTradeDto { RequestedToyId = toy.Id, TradeType = TradeType.Purchase },
            user.Id);

        var cancelResult = await service.CancelTradeAsync(createResult.Trade!.Id, otherUser.Id);

        Assert.False(cancelResult.Succeeded);
        Assert.Contains("only cancel your own", cancelResult.Message!);
    }

    // --- GetUserTradesAsync ---

    [Fact]
    public async Task GetUserTrades_ReturnsPaginatedResults()
    {
        var dbName = nameof(GetUserTrades_ReturnsPaginatedResults);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();

        context.Users.AddRange(admin, user);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        // Create 3 trades
        for (int i = 0; i < 3; i++)
        {
            var toy = TestDbHelper.CreateToy(admin.Id, $"Toy {i}");
            context.Toys.Add(toy);
            await context.SaveChangesAsync();

            await service.CreateTradeAsync(
                new CreateTradeDto { RequestedToyId = toy.Id, TradeType = TradeType.Purchase },
                user.Id);
        }

        var result = await service.GetUserTradesAsync(user.Id,
            new TradeQueryParameters { PageNumber = 1, PageSize = 2 });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetUserTrades_FiltersbyStatus()
    {
        var dbName = nameof(GetUserTrades_FiltersbyStatus);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();

        context.Users.AddRange(admin, user);
        await context.SaveChangesAsync();

        var service = new TradeService(context, _loggerMock.Object, _emailMock.Object);

        // Create 2 trades, cancel one
        var toy1 = TestDbHelper.CreateToy(admin.Id, "Toy 1");
        var toy2 = TestDbHelper.CreateToy(admin.Id, "Toy 2");
        context.Toys.AddRange(toy1, toy2);
        await context.SaveChangesAsync();

        var trade1 = await service.CreateTradeAsync(
            new CreateTradeDto { RequestedToyId = toy1.Id, TradeType = TradeType.Purchase }, user.Id);
        await service.CreateTradeAsync(
            new CreateTradeDto { RequestedToyId = toy2.Id, TradeType = TradeType.Purchase }, user.Id);

        await service.CancelTradeAsync(trade1.Trade!.Id, user.Id);

        var pendingResult = await service.GetUserTradesAsync(user.Id,
            new TradeQueryParameters { Status = TradeStatus.Pending });

        Assert.Single(pendingResult.Items);
    }
}
