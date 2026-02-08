using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RePlay.Application.Interfaces;
using RePlay.Domain.Enums;
using RePlay.Infrastructure.Data;
using Stripe;
using Stripe.Checkout;

namespace RePlay.Infrastructure.Services;

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext context,
        IOptions<StripeSettings> stripeSettings,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _stripeSettings = stripeSettings.Value;
        _logger = logger;

        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<PaymentResult> CreateCheckoutSessionAsync(Guid tradeId, Guid userId)
    {
        var trade = await _context.Trades
            .Include(t => t.RequestedToy)
            .FirstOrDefaultAsync(t => t.Id == tradeId);

        if (trade == null)
            return PaymentResult.Failure("Trade not found.");

        if (trade.UserId != userId)
            return PaymentResult.Failure("You can only pay for your own trades.");

        if (trade.TradeType != TradeType.Purchase)
            return PaymentResult.Failure("Only purchase trades require payment.");

        if (trade.Status != TradeStatus.Pending)
            return PaymentResult.Failure($"Trade is not in a payable state. Current status: {trade.Status}.");

        if (!string.IsNullOrEmpty(trade.StripePaymentIntentId))
            return PaymentResult.Failure("Payment has already been initiated for this trade.");

        var toy = trade.RequestedToy;
        var priceInCents = (long)(toy.Price * 100);

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "php",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = toy.Name,
                            Description = $"{toy.Category} - Condition: {toy.Condition}"
                        },
                        UnitAmount = priceInCents
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = $"http://localhost:4200/trades/{tradeId}/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"http://localhost:4200/trades/{tradeId}/cancel",
            Metadata = new Dictionary<string, string>
            {
                { "tradeId", tradeId.ToString() },
                { "userId", userId.ToString() }
            }
        };

        try
        {
            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation(
                "Stripe checkout session created: {SessionId} for trade {TradeId}",
                session.Id, tradeId);

            return PaymentResult.Success(checkoutUrl: session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating checkout session for trade {TradeId}", tradeId);
            return PaymentResult.Failure($"Payment processing error: {ex.Message}");
        }
    }

    public async Task<PaymentResult> HandleWebhookAsync(string payload, string signature)
    {
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signature, _stripeSettings.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Invalid Stripe webhook signature.");
            return PaymentResult.Failure("Invalid webhook signature.");
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null)
                return PaymentResult.Failure("Invalid session data.");

            return await HandleCheckoutSessionCompleted(session);
        }

        if (stripeEvent.Type == EventTypes.CheckoutSessionExpired)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null)
                return PaymentResult.Failure("Invalid session data.");

            return await HandleCheckoutSessionExpired(session);
        }

        // Unhandled event type — acknowledge receipt
        _logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
        return PaymentResult.Success();
    }

    private async Task<PaymentResult> HandleCheckoutSessionCompleted(Session session)
    {
        if (!session.Metadata.TryGetValue("tradeId", out var tradeIdStr) ||
            !Guid.TryParse(tradeIdStr, out var tradeId))
        {
            _logger.LogWarning("Stripe webhook missing tradeId metadata.");
            return PaymentResult.Failure("Missing trade reference.");
        }

        var trade = await _context.Trades
            .Include(t => t.RequestedToy)
            .FirstOrDefaultAsync(t => t.Id == tradeId);

        if (trade == null)
        {
            _logger.LogWarning("Trade {TradeId} not found for Stripe webhook.", tradeId);
            return PaymentResult.Failure("Trade not found.");
        }

        if (trade.Status != TradeStatus.Pending)
        {
            _logger.LogInformation("Trade {TradeId} already processed (status: {Status}).", tradeId, trade.Status);
            return PaymentResult.Success();
        }

        // Update trade with payment info
        trade.StripePaymentIntentId = session.PaymentIntentId;
        trade.AmountPaid = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100m : trade.AmountPaid;
        trade.Status = TradeStatus.Completed;
        trade.CompletedAt = DateTime.UtcNow;

        // Update toy status
        var toy = trade.RequestedToy;
        toy.Status = ToyStatus.Sold;
        toy.CurrentHolderId = trade.UserId;
        toy.UpdatedAt = DateTime.UtcNow;

        // Record transaction history
        var transaction = new Domain.Entities.TransactionHistory
        {
            Id = Guid.NewGuid(),
            UserId = trade.UserId,
            Type = TransactionType.Purchase,
            ToyId = trade.RequestedToyId,
            RelatedTradeId = trade.Id,
            Description = $"Purchased {toy.Name} for ₱{trade.AmountPaid:F2}",
            AmountPaid = trade.AmountPaid,
            CreatedAt = DateTime.UtcNow
        };

        _context.TransactionHistories.Add(transaction);

        // Update user's trade count
        var user = await _context.Users.FindAsync(trade.UserId);
        if (user != null)
        {
            user.TotalTradesCompleted++;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Payment completed for trade {TradeId}. Toy {ToyName} sold to user {UserId}.",
            tradeId, toy.Name, trade.UserId);

        return PaymentResult.Success(paymentIntentId: session.PaymentIntentId);
    }

    private async Task<PaymentResult> HandleCheckoutSessionExpired(Session session)
    {
        if (!session.Metadata.TryGetValue("tradeId", out var tradeIdStr) ||
            !Guid.TryParse(tradeIdStr, out var tradeId))
        {
            return PaymentResult.Success();
        }

        var trade = await _context.Trades.FindAsync(tradeId);
        if (trade != null && trade.Status == TradeStatus.Pending)
        {
            trade.Status = TradeStatus.Cancelled;
            trade.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Trade {TradeId} cancelled due to expired checkout session.", tradeId);
        }

        return PaymentResult.Success();
    }
}
