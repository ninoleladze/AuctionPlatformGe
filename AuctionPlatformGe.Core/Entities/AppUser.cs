namespace AuctionPlatformGe.Core.Entities;

public class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // User | Admin
    public string? StripeCustomerId { get; set; }
    public string? StripeAccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? PhoneNumber { get; set; }
    public bool PhoneVerified { get; set; }

    public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
    public Wallet? Wallet { get; set; }
}