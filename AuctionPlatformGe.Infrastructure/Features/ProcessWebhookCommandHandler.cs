using AuctionPlatformGe.Application.Features.Payments.Commands.ProcessWebhook;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using AuctionPlatformGe.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuctionPlatformGe.Infrastructure.Features;

public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, ProcessWebhookResult>
{
    private readonly AppDbContext _db;
    private readonly IWalletRepository _wallets;
    private readonly ILogger<ProcessWebhookCommandHandler> _logger;

    public ProcessWebhookCommandHandler(
        AppDbContext db,
        IWalletRepository wallets,
        ILogger<ProcessWebhookCommandHandler> logger)
    {
        _db = db;
        _wallets = wallets;
        _logger = logger;
    }

    public async Task<ProcessWebhookResult> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        var pending = await _db.PendingPayments
            .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (pending is null)
        {
            _logger.LogWarning("ProcessWebhook: unknown transactionId {TxId}", request.TransactionId);
            return new ProcessWebhookResult(false);
        }

        if (pending.Status == "Completed")
        {
            _logger.LogInformation("ProcessWebhook: already completed {TxId}", request.TransactionId);
            return new ProcessWebhookResult(true);
        }

        return pending.Purpose == "BuyNow"
            ? await HandleBuyNowAsync(pending, cancellationToken)
            : await HandleBidPurchaseAsync(pending, request.Amount, cancellationToken);
    }

    private async Task<ProcessWebhookResult> HandleBidPurchaseAsync(
        PendingPayment pending, decimal amount, CancellationToken cancellationToken)
    {
        var package    = Application.Models.BidPackages.GetById(pending.BidPackageId);
        var bidCredits = package?.BidCount ?? DeriveCreditsFromAmount(amount);

        var wallet = await _wallets.GetByUserIdAsync(pending.UserId);
        if (wallet is null)
        {
            _logger.LogError("ProcessWebhook: no wallet for user {UserId}", pending.UserId);
            pending.Status = "Failed";
            await _db.SaveChangesAsync(cancellationToken);
            return new ProcessWebhookResult(false);
        }

        wallet.BidBalance += bidCredits;
        wallet.GelBalance += amount;
        wallet.Transactions.Add(new WalletTransaction
        {
            WalletId    = wallet.Id,
            Type        = "BidPurchase",
            BidDelta    = bidCredits,
            GelAmount   = amount,
            Description = $"Purchased {bidCredits} bids via Payze (tx: {pending.TransactionId})",
            CreatedAt   = DateTime.UtcNow
        });

        pending.Status      = "Completed";
        pending.CompletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ProcessWebhook: credited {Credits} bids to user {UserId} for tx {TxId}",
            bidCredits, pending.UserId, pending.TransactionId);

        return new ProcessWebhookResult(true);
    }

    private async Task<ProcessWebhookResult> HandleBuyNowAsync(
        PendingPayment pending, CancellationToken cancellationToken)
    {
        if (pending.AuctionId is null)
        {
            _logger.LogError("ProcessWebhook: BuyNow pending {TxId} has no AuctionId", pending.TransactionId);
            pending.Status = "Failed";
            await _db.SaveChangesAsync(cancellationToken);
            return new ProcessWebhookResult(false);
        }

        var auction = await _db.Auctions.FindAsync([pending.AuctionId.Value], cancellationToken);
        if (auction is null || auction.Status != "Active")
        {
            _logger.LogWarning("ProcessWebhook: BuyNow auction {Id} not found or not Active", pending.AuctionId);
            pending.Status = "Failed";
            await _db.SaveChangesAsync(cancellationToken);
            return new ProcessWebhookResult(false);
        }

        auction.Status            = "Ended";
        auction.HighestBidderId   = pending.UserId;
        auction.CurrentBid        = pending.AmountGel;
        pending.Status            = "Completed";
        pending.CompletedAt       = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ProcessWebhook: BuyNow — auction {AuctionId} ended, winner {UserId}, price {Amount}",
            auction.Id, pending.UserId, pending.AmountGel);

        return new ProcessWebhookResult(true, auction.Id.ToString(), auction.CurrentBid, pending.UserId);
    }

    private static int DeriveCreditsFromAmount(decimal amount) => amount switch
    {
        <= 10m  => 10,
        <= 22m  => 25,
        <= 40m  => 50,
        _       => 100
    };
}
