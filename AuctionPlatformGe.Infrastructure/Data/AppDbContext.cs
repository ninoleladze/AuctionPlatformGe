using AuctionPlatformGe.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatformGe.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<PendingPayment> PendingPayments { get; set; }
    public DbSet<AutoBid> AutoBids { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── AppUser ──────────────────────────────────────────────
        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Auction ──────────────────────────────────────────────
        // Explicitly map SellerId → Users to eliminate shadow AppUserId FK
        modelBuilder.Entity<Auction>()
            .HasOne<AppUser>()
            .WithMany(u => u.Auctions)
            .HasForeignKey(a => a.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        // HighestBidderId — unidirectional, no cascade
        modelBuilder.Entity<Auction>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(a => a.HighestBidderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Auction>()
            .Property(a => a.StartingPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Auction>()
            .Property(a => a.CurrentBid).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Auction>()
            .Property(a => a.ReservePrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Auction>()
            .Property(a => a.BuyNowPrice).HasColumnType("decimal(18,2)");

        // Optimistic concurrency token
        modelBuilder.Entity<Auction>()
            .Property(a => a.RowVersion)
            .IsRowVersion();

        // ── Bid ──────────────────────────────────────────────────
        modelBuilder.Entity<Bid>()
            .Property(b => b.Amount).HasColumnType("decimal(18,2)");

        // BidderId — FK to Users, no cascade
        modelBuilder.Entity<Bid>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Wallet ───────────────────────────────────────────────
        modelBuilder.Entity<Wallet>()
            .Property(w => w.GelBalance).HasColumnType("decimal(18,2)");

        // ── WalletTransaction ─────────────────────────────────────
        modelBuilder.Entity<WalletTransaction>()
            .Property(t => t.GelAmount).HasColumnType("decimal(18,2)");

        // ── PendingPayment ────────────────────────────────────────
        modelBuilder.Entity<PendingPayment>()
            .Property(p => p.AmountGel).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<PendingPayment>()
            .HasIndex(p => p.TransactionId).IsUnique();

        // ── AutoBid ───────────────────────────────────────────────
        modelBuilder.Entity<AutoBid>()
            .Property(ab => ab.MaxAmount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<AutoBid>()
            .HasOne(ab => ab.Auction)
            .WithMany(a => a.AutoBids)
            .HasForeignKey(ab => ab.AuctionId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<AutoBid>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(ab => ab.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<AutoBid>()
            .HasIndex(ab => new { ab.AuctionId, ab.UserId });
    }
}
