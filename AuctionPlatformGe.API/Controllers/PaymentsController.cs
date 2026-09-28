using AuctionPlatformGe.Application.Features.Payments.Commands.InitiateBuyNow;
using AuctionPlatformGe.Application.Features.Payments.Commands.InitiatePayment;
using AuctionPlatformGe.Application.Features.Payments.Commands.ProcessWebhook;
using AuctionPlatformGe.Application.Features.Payments.Queries.GetBidPackages;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.API.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AuctionPlatformGe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentGateway _gateway;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IConfiguration _config;
    private readonly IHubContext<BidHub> _hub;

    public PaymentsController(
        IMediator mediator,
        IPaymentGateway gateway,
        ILogger<PaymentsController> logger,
        IConfiguration config,
        IHubContext<BidHub> hub)
    {
        _mediator = mediator;
        _gateway  = gateway;
        _logger   = logger;
        _config   = config;
        _hub      = hub;
    }

    /// <summary>List available bid packages.</summary>
    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages() =>
        Ok(await _mediator.Send(new GetBidPackagesQuery()));

    /// <summary>Create a Payze payment session for a bid package.</summary>
    [HttpPost("initiate")]
    [Authorize]
    public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
    {
        var userId      = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var apiBase     = $"{Request.Scheme}://{Request.Host}";
        var callbackUrl = $"{apiBase}/api/payments/webhook/payze";
        var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
        var returnUrl   = $"{frontendUrl}/wallet?status=success";

        var result = await _mediator.Send(
            new InitiatePaymentCommand(userId, dto.PackageId, returnUrl, callbackUrl));

        return Ok(result);
    }

    /// <summary>Create a Payze payment session for a Buy Now purchase.</summary>
    [HttpPost("buy-now")]
    [Authorize]
    public async Task<IActionResult> BuyNow([FromBody] BuyNowDto dto)
    {
        var userId      = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var apiBase     = $"{Request.Scheme}://{Request.Host}";
        var callbackUrl = $"{apiBase}/api/payments/webhook/payze";
        var frontendUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:4200";
        var returnUrl   = $"{frontendUrl}/auctions/{dto.AuctionId}?status=buynow-success";

        try
        {
            var result = await _mediator.Send(
                new InitiateBuyNowCommand(userId, dto.AuctionId, returnUrl, callbackUrl));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>Payze webhook — called by Payze on successful payment.</summary>
    [HttpPost("webhook/payze")]
    public async Task<IActionResult> PayzeWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync();

        var signature = Request.Headers["X-Payze-Signature"].ToString();

        if (!_gateway.VerifyWebhookSignature(rawBody, signature))
        {
            _logger.LogWarning("Payze webhook: invalid signature");
            return Unauthorized();
        }

        var parsed = _gateway.ParseWebhook(rawBody);

        if (!parsed.IsSuccess)
        {
            _logger.LogInformation("Payze webhook: non-success status {Status} for tx {TxId}",
                parsed.RawStatus, parsed.TransactionId);
            return Ok();
        }

        var result = await _mediator.Send(new ProcessWebhookCommand(parsed.TransactionId, parsed.Amount));

        if (result.BuyNowAuctionId is not null)
        {
            await _hub.Clients.Group($"auction-{result.BuyNowAuctionId}")
                .SendAsync("AuctionEnded", new
                {
                    auctionId        = result.BuyNowAuctionId,
                    finalBid         = result.FinalBid,
                    highestBidderId  = result.WinnerId,
                    reserveNotMet    = false
                });
        }

        return Ok();
    }

    /// <summary>Test-mode only: simulate a completed payment (redirected from Payze test flow).</summary>
    [HttpGet("test-complete")]
    public async Task<IActionResult> TestComplete([FromQuery] string transactionId)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IConfiguration>()
                .GetValue<bool>("Payze:TestMode"))
            return NotFound();

        var db = HttpContext.RequestServices
            .GetRequiredService<AuctionPlatformGe.Infrastructure.Data.AppDbContext>();

        var pending = db.PendingPayments
            .FirstOrDefault(p => p.TransactionId == transactionId);

        if (pending is null) return BadRequest("Unknown transaction");

        var result = await _mediator.Send(new ProcessWebhookCommand(transactionId, pending.AmountGel));

        if (result.BuyNowAuctionId is not null)
        {
            await _hub.Clients.Group($"auction-{result.BuyNowAuctionId}")
                .SendAsync("AuctionEnded", new
                {
                    auctionId        = result.BuyNowAuctionId,
                    finalBid         = result.FinalBid,
                    highestBidderId  = result.WinnerId,
                    reserveNotMet    = false
                });
        }

        var frontendBase = HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["Frontend:BaseUrl"] ?? "http://localhost:4200";

        var redirectUrl = result.BuyNowAuctionId is not null
            ? $"{frontendBase}/auctions/{result.BuyNowAuctionId}?status=buynow-success"
            : $"{frontendBase}/wallet?status=success&tx={transactionId}";

        return Redirect(redirectUrl);
    }
}

public record InitiatePaymentDto(int PackageId);
public record BuyNowDto(string AuctionId);
