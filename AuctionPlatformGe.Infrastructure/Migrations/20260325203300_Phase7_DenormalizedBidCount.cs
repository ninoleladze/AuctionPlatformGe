using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionPlatformGe.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase7_DenormalizedBidCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BidCount",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill existing auctions from the Bids table
            migrationBuilder.Sql("""
                UPDATE Auctions
                SET BidCount = (SELECT COUNT(*) FROM Bids WHERE Bids.AuctionId = Auctions.Id)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BidCount",
                table: "Auctions");
        }
    }
}
