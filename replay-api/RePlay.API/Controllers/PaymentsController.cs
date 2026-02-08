using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RePlay.Application.Interfaces;

namespace RePlay.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a Stripe checkout session for a purchase trade
    /// </summary>
    [HttpPost("create-checkout")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaymentResult>> CreateCheckoutSession([FromBody] CreateCheckoutRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var result = await _paymentService.CreateCheckoutSessionAsync(request.TradeId, userId);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok(result);
    }

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without signature header.");
            return BadRequest(new { message = "Missing Stripe-Signature header." });
        }

        var result = await _paymentService.HandleWebhookAsync(payload, signature);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Message });

        return Ok();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class CreateCheckoutRequest
{
    public Guid TradeId { get; set; }
}
