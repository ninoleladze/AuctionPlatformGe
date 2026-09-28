using AuctionPlatformGe.Application.Interfaces;
using MediatR;

namespace AuctionPlatformGe.Application.Features.AutoBids.Commands.CancelAutoBid;

public class CancelAutoBidCommandHandler : IRequestHandler<CancelAutoBidCommand, Unit>
{
    private readonly IAutoBidRepository _autoBids;

    public CancelAutoBidCommandHandler(IAutoBidRepository autoBids)
    {
        _autoBids = autoBids;
    }

    public async Task<Unit> Handle(CancelAutoBidCommand request, CancellationToken cancellationToken)
    {
        var auctionId = Guid.Parse(request.AuctionId);
        var autoBid = await _autoBids.GetActiveAsync(auctionId, request.UserId);
        if (autoBid is not null)
        {
            autoBid.IsActive = false;
            _autoBids.Update(autoBid);
            await _autoBids.SaveChangesAsync();
        }
        return Unit.Value;
    }
}
