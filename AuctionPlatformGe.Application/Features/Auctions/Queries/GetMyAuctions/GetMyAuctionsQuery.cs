using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Queries.GetMyAuctions;

public record GetMyAuctionsQuery(string SellerId) : IRequest<List<AuctionListDto>>;
