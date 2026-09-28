namespace AuctionPlatformGe.Core.Entities;

public class Bid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;
    public string BidderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsAutoBid { get; set; }
    public string? IpAddress { get; set; }
    public DateTime PlacedAt { get; set; } = DateTime.UtcNow;
}