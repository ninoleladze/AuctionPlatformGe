namespace AuctionPlatformGe.Core.Entities;

public class AutoBid
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Auction Auction { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public decimal MaxAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
