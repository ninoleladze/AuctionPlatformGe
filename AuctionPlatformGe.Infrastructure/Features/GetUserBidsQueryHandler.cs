using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Features.Bids.Queries.GetUserBids;
using AuctionPlatformGe.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Features;

public class GetUserBidsQueryHandler : IRequestHandler<GetUserBidsQuery, PaginatedResult<UserBidDto>>
{
    private readonly AppDbContext _db;

    public GetUserBidsQueryHandler(AppDbContext db) { _db = db; }

    public async Task<PaginatedResult<UserBidDto>> Handle(
        GetUserBidsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Bids.Where(b => b.BidderId == request.UserId);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(b => b.PlacedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new UserBidDto(b.Id, b.AuctionId, b.Auction.Title, b.Amount, b.PlacedAt))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<UserBidDto>(items, total, request.Page, request.PageSize);
    }
}
