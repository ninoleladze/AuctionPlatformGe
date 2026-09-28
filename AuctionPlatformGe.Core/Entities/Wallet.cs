namespace AuctionPlatformGe.Core.Entities;

public class Wallet
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser User { get; set; } = null!;
    public int BidBalance { get; set; }       // number of bid credits
    public decimal GelBalance { get; set; }   // Georgian Lari
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
