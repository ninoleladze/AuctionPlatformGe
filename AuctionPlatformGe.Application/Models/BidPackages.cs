namespace AuctionPlatformGe.Application.Models;

public record BidPackage(int Id, int BidCount, decimal PriceGel, string Name);

public static class BidPackages
{
    public static readonly IReadOnlyList<BidPackage> All =
    [
        new(1, 10,  10m,  "Starter Pack"),
        new(2, 25,  22m,  "Value Pack"),
        new(3, 50,  40m,  "Pro Pack"),
        new(4, 100, 70m,  "Ultimate Pack")
    ];

    public static BidPackage? GetById(int id) => All.FirstOrDefault(p => p.Id == id);
}
