namespace AuctionPlatformGe.Application.DTOs;

public record BidDto(Guid Id, string BidderId, decimal Amount, DateTime PlacedAt);
