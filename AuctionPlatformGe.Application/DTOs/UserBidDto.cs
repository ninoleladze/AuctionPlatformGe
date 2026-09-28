namespace AuctionPlatformGe.Application.DTOs;

public record UserBidDto(Guid BidId, Guid AuctionId, string AuctionTitle, decimal Amount, DateTime PlacedAt);
