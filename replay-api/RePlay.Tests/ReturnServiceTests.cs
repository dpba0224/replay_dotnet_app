using Microsoft.Extensions.Logging;
using Moq;
using RePlay.Application.Interfaces;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Services;

namespace RePlay.Tests;

public class ReturnServiceTests
{
    private readonly Mock<ILogger<ReturnService>> _loggerMock = new();

    // --- InitiateReturnAsync ---

    [Fact]
    public async Task InitiateReturn_ValidTradedToy_Succeeds()
    {
        var dbName = nameof(InitiateReturn_ValidTradedToy_Succeeds);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);
        var dto = new CreateReturnDto { ToyId = toy.Id, UserNotes = "Returning this toy" };

        var result = await service.InitiateReturnAsync(dto, user.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Return);
        Assert.Equal("Pending", result.Return!.Status);
        Assert.Equal("Returning this toy", result.Return.UserNotes);

        // Verify toy status changed to PendingReturn
        var updatedToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.PendingReturn, updatedToy!.Status);
    }

    [Fact]
    public async Task InitiateReturn_ValidSoldToy_Succeeds()
    {
        var dbName = nameof(InitiateReturn_ValidSoldToy_Succeeds);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Sold Toy", ToyStatus.Sold, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);
        var result = await service.InitiateReturnAsync(new CreateReturnDto { ToyId = toy.Id }, user.Id);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task InitiateReturn_ToyNotFound_Fails()
    {
        var dbName = nameof(InitiateReturn_ToyNotFound_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var user = TestDbHelper.CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);
        var result = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = Guid.NewGuid() }, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Message!);
    }

    [Fact]
    public async Task InitiateReturn_NotHolder_Fails()
    {
        var dbName = nameof(InitiateReturn_NotHolder_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var otherUser = TestDbHelper.CreateUser("Other", "other@test.com");
        var toy = TestDbHelper.CreateToy(admin.Id, "Someone Else's Toy", ToyStatus.Traded, holderId: otherUser.Id);

        context.Users.AddRange(admin, user, otherUser);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);
        var result = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("only return toys that you currently hold", result.Message!);
    }

    [Fact]
    public async Task InitiateReturn_AvailableToy_Fails()
    {
        var dbName = nameof(InitiateReturn_AvailableToy_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Available Toy", ToyStatus.Available, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);
        var result = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("cannot be returned", result.Message!);
    }

    [Fact]
    public async Task InitiateReturn_DuplicatePending_Fails()
    {
        var dbName = nameof(InitiateReturn_DuplicatePending_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        // First return succeeds (toy goes to PendingReturn)
        var result1 = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);
        Assert.True(result1.Succeeded);

        // Second return fails because toy is now PendingReturn (not Traded/Sold)
        var result2 = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);
        Assert.False(result2.Succeeded);
        Assert.Contains("cannot be returned", result2.Message!);
    }

    // --- ApproveReturnAsync ---

    [Fact]
    public async Task ApproveReturn_Succeeds_ResetstoAvailable()
    {
        var dbName = nameof(ApproveReturn_Succeeds_ResetstoAvailable);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        // Initiate return
        var initiateResult = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        // Approve with re-rated condition
        var approveResult = await service.ApproveReturnAsync(
            initiateResult.Return!.Id,
            new ApproveReturnDto
            {
                ConditionOnReturn = ToyCondition.Fair,
                AdminNotes = "Some wear noted"
            },
            admin.Id);

        Assert.True(approveResult.Succeeded);
        Assert.Equal("Approved", approveResult.Return!.Status);

        // Verify toy is back to Available with updated condition
        var updatedToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.Available, updatedToy!.Status);
        Assert.Equal(ToyCondition.Fair, updatedToy.Condition);
        Assert.Null(updatedToy.CurrentHolderId);

        // Verify transaction history was recorded
        Assert.Single(context.TransactionHistories);
    }

    [Fact]
    public async Task ApproveReturn_WithUserRating_UpdatesReputation()
    {
        var dbName = nameof(ApproveReturn_WithUserRating_UpdatesReputation);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        var initiateResult = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        var approveResult = await service.ApproveReturnAsync(
            initiateResult.Return!.Id,
            new ApproveReturnDto
            {
                ConditionOnReturn = ToyCondition.Good,
                UserRating = 4,
                RatingComment = "Good care of the toy"
            },
            admin.Id);

        Assert.True(approveResult.Succeeded);

        // Verify rating was created
        Assert.Single(context.Ratings);
        var rating = context.Ratings.First();
        Assert.Equal(4, rating.Score);
        Assert.Equal(user.Id, rating.RatedUserId);

        // Verify user reputation was updated
        var updatedUser = await context.Users.FindAsync(user.Id);
        Assert.True(updatedUser!.ReputationScore > 0);
    }

    [Fact]
    public async Task ApproveReturn_NotPending_Fails()
    {
        var dbName = nameof(ApproveReturn_NotPending_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        var initiateResult = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        // Approve once
        await service.ApproveReturnAsync(initiateResult.Return!.Id,
            new ApproveReturnDto { ConditionOnReturn = ToyCondition.Good }, admin.Id);

        // Try to approve again
        var secondApprove = await service.ApproveReturnAsync(initiateResult.Return.Id,
            new ApproveReturnDto { ConditionOnReturn = ToyCondition.Good }, admin.Id);

        Assert.False(secondApprove.Succeeded);
        Assert.Contains("cannot be approved", secondApprove.Message!);
    }

    [Fact]
    public async Task ApproveReturn_NotFound_Fails()
    {
        var dbName = nameof(ApproveReturn_NotFound_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var service = new ReturnService(context, _loggerMock.Object);

        var result = await service.ApproveReturnAsync(Guid.NewGuid(),
            new ApproveReturnDto { ConditionOnReturn = ToyCondition.Good }, Guid.NewGuid());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Message!);
    }

    // --- RejectReturnAsync ---

    [Fact]
    public async Task RejectReturn_Succeeds_RevertsToyStatus()
    {
        var dbName = nameof(RejectReturn_Succeeds_RevertsToyStatus);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        var initiateResult = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        // Verify toy is PendingReturn
        var pendingToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.PendingReturn, pendingToy!.Status);

        // Reject the return
        var rejectResult = await service.RejectReturnAsync(
            initiateResult.Return!.Id, "Toy not eligible for return", admin.Id);

        Assert.True(rejectResult.Succeeded);
        Assert.Equal("Rejected", rejectResult.Return!.Status);

        // Verify toy reverted back to Traded
        var updatedToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.Traded, updatedToy!.Status);
        Assert.Equal(user.Id, updatedToy.CurrentHolderId);

        // Verify transaction history was recorded
        Assert.Single(context.TransactionHistories);
    }

    [Fact]
    public async Task RejectReturn_NotPending_Fails()
    {
        var dbName = nameof(RejectReturn_NotPending_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Traded Toy", ToyStatus.Traded, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        var initiateResult = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id }, user.Id);

        // Reject once
        await service.RejectReturnAsync(initiateResult.Return!.Id, "No", admin.Id);

        // Try to reject again
        var secondReject = await service.RejectReturnAsync(
            initiateResult.Return.Id, "Double rejection", admin.Id);

        Assert.False(secondReject.Succeeded);
        Assert.Contains("cannot be rejected", secondReject.Message!);
    }

    [Fact]
    public async Task RejectReturn_NotFound_Fails()
    {
        var dbName = nameof(RejectReturn_NotFound_Fails);
        var context = TestDbHelper.CreateContext(dbName);
        var service = new ReturnService(context, _loggerMock.Object);

        var result = await service.RejectReturnAsync(Guid.NewGuid(), "notes", Guid.NewGuid());

        Assert.False(result.Succeeded);
        Assert.Contains("not found", result.Message!);
    }

    // --- GetUserReturnsAsync ---

    [Fact]
    public async Task GetUserReturns_ReturnsPaginatedResults()
    {
        var dbName = nameof(GetUserReturns_ReturnsPaginatedResults);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();

        context.Users.AddRange(admin, user);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        // Create 3 returns
        for (int i = 0; i < 3; i++)
        {
            var toy = TestDbHelper.CreateToy(admin.Id, $"Toy {i}", ToyStatus.Traded, holderId: user.Id);
            context.Toys.Add(toy);
            await context.SaveChangesAsync();

            await service.InitiateReturnAsync(new CreateReturnDto { ToyId = toy.Id }, user.Id);
        }

        var result = await service.GetUserReturnsAsync(user.Id,
            new ReturnQueryParameters { PageNumber = 1, PageSize = 2 });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetAllReturns_ReturnsAllUsers()
    {
        var dbName = nameof(GetAllReturns_ReturnsAllUsers);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user1 = TestDbHelper.CreateUser("User 1", "user1@test.com");
        var user2 = TestDbHelper.CreateUser("User 2", "user2@test.com");
        var toy1 = TestDbHelper.CreateToy(admin.Id, "Toy 1", ToyStatus.Traded, holderId: user1.Id);
        var toy2 = TestDbHelper.CreateToy(admin.Id, "Toy 2", ToyStatus.Sold, holderId: user2.Id);

        context.Users.AddRange(admin, user1, user2);
        context.Toys.AddRange(toy1, toy2);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        await service.InitiateReturnAsync(new CreateReturnDto { ToyId = toy1.Id }, user1.Id);
        await service.InitiateReturnAsync(new CreateReturnDto { ToyId = toy2.Id }, user2.Id);

        var result = await service.GetAllReturnsAsync(new ReturnQueryParameters());

        Assert.Equal(2, result.TotalCount);
    }

    // --- Full Return Flow (Integration) ---

    [Fact]
    public async Task FullReturnFlow_InitiateApprove_ToyBackInInventory()
    {
        var dbName = nameof(FullReturnFlow_InitiateApprove_ToyBackInInventory);
        var context = TestDbHelper.CreateContext(dbName);
        var admin = TestDbHelper.CreateAdmin();
        var user = TestDbHelper.CreateUser();
        var toy = TestDbHelper.CreateToy(admin.Id, "Full Flow Toy", ToyStatus.Traded,
            condition: ToyCondition.Excellent, holderId: user.Id);

        context.Users.AddRange(admin, user);
        context.Toys.Add(toy);
        await context.SaveChangesAsync();

        var service = new ReturnService(context, _loggerMock.Object);

        // Step 1: User initiates return
        var initResult = await service.InitiateReturnAsync(
            new CreateReturnDto { ToyId = toy.Id, UserNotes = "Done playing with it" }, user.Id);
        Assert.True(initResult.Succeeded);

        // Step 2: Verify toy is now PendingReturn
        var pendingToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.PendingReturn, pendingToy!.Status);

        // Step 3: Admin approves with lower condition rating
        var approveResult = await service.ApproveReturnAsync(
            initResult.Return!.Id,
            new ApproveReturnDto
            {
                ConditionOnReturn = ToyCondition.Good,
                AdminNotes = "Minor scratches observed",
                UserRating = 3,
                RatingComment = "Average care"
            },
            admin.Id);
        Assert.True(approveResult.Succeeded);

        // Step 4: Verify final state
        var finalToy = await context.Toys.FindAsync(toy.Id);
        Assert.Equal(ToyStatus.Available, finalToy!.Status);
        Assert.Equal(ToyCondition.Good, finalToy.Condition); // Downgraded from Excellent
        Assert.Null(finalToy.CurrentHolderId); // No longer held

        var finalUser = await context.Users.FindAsync(user.Id);
        Assert.Equal(3m, finalUser!.ReputationScore); // Rating of 3

        Assert.Single(context.Ratings);
        Assert.Single(context.TransactionHistories);
    }
}
