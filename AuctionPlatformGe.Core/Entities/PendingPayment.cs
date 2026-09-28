namespace AuctionPlatformGe.Core.Entities;

public class PendingPayment
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty; // provider-assigned
    public int BidPackageId { get; set; }
    public int BidCredits { get; set; }
    public decimal AmountGel { get; set; }
    public string Purpose { get; set; } = "BidPurchase"; // BidPurchase | BuyNow
    public Guid? AuctionId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending | Completed | Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
