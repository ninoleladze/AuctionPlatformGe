using MediatR;

namespace AuctionPlatformGe.Application.Features.AutoBids.Commands.CancelAutoBid;

public record CancelAutoBidCommand(string AuctionId, string UserId) : IRequest<Unit>;
