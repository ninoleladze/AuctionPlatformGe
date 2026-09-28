using AuctionPlatformGe.Application.DTOs;
using AuctionPlatformGe.Application.Features.Auctions.Queries.GetMyAuctions;
using AuctionPlatformGe.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Features;

public class GetMyAuctionsQueryHandler : IRequestHandler<GetMyAuctionsQuery, List<AuctionListDto>>
{
    private readonly AppDbContext _db;

    public GetMyAuctionsQueryHandler(AppDbContext db) { _db = db; }

    public async Task<List<AuctionListDto>> Handle(
        GetMyAuctionsQuery request, CancellationToken cancellationToken) =>
        await _db.Auctions
            .Where(a => a.SellerId == request.SellerId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AuctionListDto(
                a.Id, a.Title, a.Description, a.ItemType,
                a.StartingPrice, a.CurrentBid, a.BuyNowPrice,
                a.Status, a.StartsAt, a.EndsAt, a.SellerId,
                a.BidCount, a.ImageUrl))
            .ToListAsync(cancellationToken);
}
