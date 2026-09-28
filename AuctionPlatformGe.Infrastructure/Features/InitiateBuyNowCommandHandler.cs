using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Features.Payments.Commands.InitiateBuyNow;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Features;

public class InitiateBuyNowCommandHandler : IRequestHandler<InitiateBuyNowCommand, PaymentInitDto>
{
    private readonly AppDbContext _db;
    private readonly IPaymentGateway _gateway;

    public InitiateBuyNowCommandHandler(AppDbContext db, IPaymentGateway gateway)
    {
        _db      = db;
        _gateway = gateway;
    }

    public async Task<PaymentInitDto> Handle(InitiateBuyNowCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.AuctionId, out var auctionId))
            throw new InvalidOperationException("Invalid auction ID.");

        var auction = await _db.Auctions
            .FirstOrDefaultAsync(a => a.Id == auctionId, cancellationToken)
            ?? throw new InvalidOperationException("Auction not found.");

        if (auction.Status != "Active")
            throw new InvalidOperationException("Auction is not active.");

        if (auction.BuyNowPrice is null)
            throw new InvalidOperationException("This auction has no Buy Now price.");

        if (auction.SellerId == request.UserId)
            throw new InvalidOperationException("Sellers cannot buy their own auction.");

        var transactionId = Guid.NewGuid().ToString("N");

        var result = await _gateway.InitiatePaymentAsync(new PaymentRequest(
            transactionId,
            request.UserId,
            auction.BuyNowPrice.Value,
            $"Buy Now — {auction.Title}",
            request.ReturnUrl,
            request.CallbackUrl,
            Purpose:   "BuyNow",
            AuctionId: auctionId));

        if (!result.Success)
            throw new InvalidOperationException(result.Error ?? "Payment gateway error.");

        return new PaymentInitDto(transactionId, result.PaymentUrl!);
    }
}
