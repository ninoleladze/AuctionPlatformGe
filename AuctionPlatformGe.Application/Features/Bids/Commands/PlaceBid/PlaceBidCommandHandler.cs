using System.Data;
using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Bids.Commands.PlaceBid;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, BidResultDto>
{
    private readonly IAuctionRepository _auctions;
    private readonly IBidRepository _bids;
    private readonly IWalletRepository _wallets;
    private readonly IAutoBidRepository _autoBids;
    private readonly IUnitOfWork _uow;

    public PlaceBidCommandHandler(
        IAuctionRepository auctions,
        IBidRepository bids,
        IWalletRepository wallets,
        IAutoBidRepository autoBids,
        IUnitOfWork uow)
    {
        _auctions = auctions;
        _bids = bids;
        _wallets = wallets;
        _autoBids = autoBids;
        _uow = uow;
    }

    public Task<BidResultDto> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        return _uow.ExecuteInTransactionAsync(async () =>
        {
            var auctionId = Guid.Parse(request.AuctionId);
            var auction = await _auctions.GetWithBidsAsync(auctionId)
                ?? throw new InvalidOperationException("Auction not found");

            if (auction.Status != "Active")
                throw new InvalidOperationException("Auction is not active");

            if (auction.EndsAt < DateTime.UtcNow)
                throw new InvalidOperationException("Auction has ended");

            if (request.Amount <= auction.CurrentBid)
                throw new InvalidOperationException($"Bid must be higher than {auction.CurrentBid}");

            if (auction.SellerId == request.BidderId)
                throw new InvalidOperationException("You cannot bid on your own auction");

            var wallet = await _wallets.GetByUserIdAsync(request.BidderId)
                ?? throw new InvalidOperationException("Wallet not found. Please contact support.");

            if (wallet.BidBalance < 1)
                throw new InvalidOperationException("Insufficient bid balance. Please purchase more bids.");

            // Deduct 1 bid credit for the manual bid
            wallet.BidBalance -= 1;
            wallet.Transactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = "BidUsed",
                BidDelta = -1,
                GelAmount = 0,
                Description = $"Bid placed on auction {auction.Title}",
                IpAddress = request.IpAddress,
                CreatedAt = DateTime.UtcNow
            });

            // Anti-sniping: if < 10 seconds remain, extend by 10 seconds
            bool extended = false;
            if (auction.EndsAt - DateTime.UtcNow < TimeSpan.FromSeconds(10))
            {
                auction.EndsAt = DateTime.UtcNow.AddSeconds(10);
                extended = true;
            }

            await _bids.AddAsync(new Bid
            {
                AuctionId = auctionId,
                BidderId = request.BidderId,
                Amount = request.Amount,
                IpAddress = request.IpAddress,
                PlacedAt = DateTime.UtcNow
            });

            auction.CurrentBid = request.Amount;
            auction.HighestBidderId = request.BidderId;
            auction.BidCount += 1;

            string finalBidderId = request.BidderId;
            decimal finalAmount = request.Amount;

            // --- Auto-bid processing ---
            // Find competing auto-bids that can still outbid the manual bid
            var competing = (await _autoBids.GetActiveForAuctionAsync(auctionId))
                .Where(ab => ab.UserId != request.BidderId && ab.MaxAmount > request.Amount)
                .OrderByDescending(ab => ab.MaxAmount)
                .ToList();

            if (competing.Count > 0)
            {
                var winner = competing[0];

                // Counter-amount: beat the manual bid (or the second auto-bidder's max)
                decimal counterAmount = request.Amount + 1m;
                if (competing.Count > 1)
                    counterAmount = Math.Max(counterAmount, competing[1].MaxAmount + 1m);
                counterAmount = Math.Min(counterAmount, winner.MaxAmount);

                var winnerWallet = await _wallets.GetByUserIdAsync(winner.UserId);
                if (winnerWallet is not null && winnerWallet.BidBalance >= 1)
                {
                    winnerWallet.BidBalance -= 1;
                    winnerWallet.Transactions.Add(new WalletTransaction
                    {
                        WalletId = winnerWallet.Id,
                        Type = "BidUsed",
                        BidDelta = -1,
                        GelAmount = 0,
                        Description = $"Auto-bid on auction {auction.Title}",
                        IpAddress = "auto-bid",
                        CreatedAt = DateTime.UtcNow
                    });

                    await _bids.AddAsync(new Bid
                    {
                        AuctionId = auctionId,
                        BidderId = winner.UserId,
                        Amount = counterAmount,
                        IpAddress = "auto-bid",
                        PlacedAt = DateTime.UtcNow
                    });

                    auction.CurrentBid = counterAmount;
                    auction.HighestBidderId = winner.UserId;
                    auction.BidCount += 1;
                    finalBidderId = winner.UserId;
                    finalAmount = counterAmount;
                }
            }

            await _bids.SaveChangesAsync();

            return new BidResultDto(
                request.AuctionId,
                finalAmount,
                finalBidderId,
                auction.BidCount,
                auction.EndsAt,
                extended
            );
        }, IsolationLevel.RepeatableRead);
    }
}
