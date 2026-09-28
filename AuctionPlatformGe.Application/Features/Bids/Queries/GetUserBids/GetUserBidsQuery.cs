using AuctionPlatformGe.Application.DTOs;
using MediatR;

namespace AuctionPlatformGe.Application.Features.Bids.Queries.GetUserBids;

public record GetUserBidsQuery(string UserId, int Page = 1, int PageSize = 20)
    : IRequest<PaginatedResult<UserBidDto>>;
