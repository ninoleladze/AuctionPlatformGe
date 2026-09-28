using MediatR;

namespace AuctionPlatformGe.Application.Features.AutoBids.Commands.SetAutoBid;

public record SetAutoBidCommand(string AuctionId, string UserId, decimal MaxAmount) : IRequest<Unit>;
