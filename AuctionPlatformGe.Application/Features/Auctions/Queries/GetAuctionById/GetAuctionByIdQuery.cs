using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid Id) : IRequest<AuctionDetailDto?>;
