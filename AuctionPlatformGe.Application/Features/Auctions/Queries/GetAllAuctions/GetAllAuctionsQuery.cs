using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Auctions.Queries.GetAllAuctions;

public record GetAllAuctionsQuery(int Page = 1, int PageSize = 12) : IRequest<PaginatedResult<AuctionListDto>>;
