namespace AuctionPlatformGe.Application.DTOs;

public record AuctionDetailDto(
    Guid Id,
    string Title,
    string Description,
    string ItemType,
    decimal StartingPrice,
    decimal CurrentBid,
    decimal? BuyNowPrice,
    string Status,
    DateTime StartsAt,
    DateTime EndsAt,
    string SellerId,
    string? HighestBidderId,
    int BidCount,
    IList<BidDto> Bids,
    string? ImageUrl
);
