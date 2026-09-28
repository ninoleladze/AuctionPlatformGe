using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Bids.Commands.PlaceBid;

public record PlaceBidCommand(string AuctionId, decimal Amount, string BidderId, string? IpAddress) : IRequest<BidResultDto>;
