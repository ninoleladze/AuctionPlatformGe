using AuctionPlatformGe.Application.Interfaces;
using AuctionPlatformGe.Core.Entities;
using MediatR;

namespace AuctionPlatformGe.Application.Features.AutoBids.Commands.SetAutoBid;

public class SetAutoBidCommandHandler : IRequestHandler<SetAutoBidCommand, Unit>
{
    private readonly IAutoBidRepository _autoBids;
    private readonly IAuctionRepository _auctions;

    public SetAutoBidCommandHandler(IAutoBidRepository autoBids, IAuctionRepository auctions)
    {
        _autoBids = autoBids;
        _auctions = auctions;
    }

    public async Task<Unit> Handle(SetAutoBidCommand request, CancellationToken cancellationToken)
    {
        var auctionId = Guid.Parse(request.AuctionId);
        var auction = await _auctions.GetByIdAsync(auctionId)
            ?? throw new InvalidOperationException("Auction not found");

        if (auction.Status != "Active")
            throw new InvalidOperationException("Auction is not active");

        if (auction.SellerId == request.UserId)
            throw new InvalidOperationException("You cannot set an auto-bid on your own auction");

        if (request.MaxAmount <= auction.CurrentBid)
            throw new InvalidOperationException($"Max amount must be greater than current bid of {auction.CurrentBid}");

        var existing = await _autoBids.GetActiveAsync(auctionId, request.UserId);
        if (existing is not null)
        {
            existing.MaxAmount = request.MaxAmount;
            _autoBids.Update(existing);
        }
        else
        {
            await _autoBids.AddAsync(new AutoBid
            {
                AuctionId = auctionId,
                UserId = request.UserId,
                MaxAmount = request.MaxAmount
            });
        }

        await _autoBids.SaveChangesAsync();
        return Unit.Value;
    }
}
