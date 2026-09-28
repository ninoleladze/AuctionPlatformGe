using MediatR;

namespace AuctionPlatformGe.Application.Features.Payments.Commands.ProcessWebhook;

public record ProcessWebhookCommand(string TransactionId, decimal Amount) : IRequest<ProcessWebhookResult>;

/// <summary>
/// Carries auction info back to the controller when a Buy Now payment completes,
/// so the controller can broadcast AuctionEnded via SignalR.
/// </summary>
public record ProcessWebhookResult(
    bool Success,
    string? BuyNowAuctionId = null,
    decimal? FinalBid       = null,
    string? WinnerId        = null);
