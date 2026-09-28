namespace AuctionPlatformGe.Application.DTOs;

public record AuctionListDto(
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
    int BidCount,
    string? ImageUrl
);
