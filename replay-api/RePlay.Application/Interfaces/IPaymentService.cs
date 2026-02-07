namespace RePlay.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> CreateCheckoutSessionAsync(Guid tradeId, Guid userId);
    Task<PaymentResult> HandleWebhookAsync(string payload, string signature);
}

public class PaymentResult
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? PaymentIntentId { get; set; }

    public static PaymentResult Success(string? checkoutUrl = null, string? paymentIntentId = null)
        => new() { Succeeded = true, CheckoutUrl = checkoutUrl, PaymentIntentId = paymentIntentId };

    public static PaymentResult Failure(string message)
        => new() { Succeeded = false, Message = message };
}
