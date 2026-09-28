namespace AuctionPlatformGe.Application.DTOs;

public record BidResultDto(
    string AuctionId,
    decimal NewBid,
    string BidderId,
    int BidCount,
    DateTime EndsAt,
    bool Extended
);
