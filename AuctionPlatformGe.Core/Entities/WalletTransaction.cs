namespace AuctionPlatformGe.Core.Entities;

public class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // BidPurchase, BidUsed, BidRefund, Deposit, Withdrawal
    public int BidDelta { get; set; }       // positive = credit, negative = debit
    public decimal GelAmount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
