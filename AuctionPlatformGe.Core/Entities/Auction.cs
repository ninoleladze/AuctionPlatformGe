namespace AuctionPlatformGe.Core.Entities;

public class Auction
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty; // Physical, Digital, GameItem
    public decimal StartingPrice { get; set; }
    public decimal? ReservePrice { get; set; }
    public decimal? BuyNowPrice { get; set; }
    public decimal CurrentBid { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Active, Ended, Cancelled
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string SellerId { get; set; } = string.Empty;
    public string? HighestBidderId { get; set; }
    public string? ImageUrl { get; set; }
    public int BidCount { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<AutoBid> AutoBids { get; set; } = new List<AutoBid>();
}