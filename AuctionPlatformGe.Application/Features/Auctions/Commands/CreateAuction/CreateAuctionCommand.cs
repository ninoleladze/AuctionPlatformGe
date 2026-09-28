using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Commands.CreateAuction;

public record CreateAuctionCommand(
    string Title,
    string Description,
    string ItemType,
    decimal StartingPrice,
    decimal? ReservePrice,
    decimal? BuyNowPrice,
    DateTime StartsAt,
    DateTime EndsAt,
    string SellerId,
    string? ImageUrl = null
) : IRequest<AuctionDetailDto>;
