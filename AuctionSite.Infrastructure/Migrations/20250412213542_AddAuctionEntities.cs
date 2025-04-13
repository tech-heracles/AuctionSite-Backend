using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuctionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Bids",
                newName: "PlacedAt");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentHighestBid",
                table: "Auctions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HighestBidderId",
                table: "Auctions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_HighestBidderId",
                table: "Auctions",
                column: "HighestBidderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Users_HighestBidderId",
                table: "Auctions",
                column: "HighestBidderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Users_HighestBidderId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_HighestBidderId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "HighestBidderId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Auctions");

            migrationBuilder.RenameColumn(
                name: "PlacedAt",
                table: "Bids",
                newName: "Timestamp");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentHighestBid",
                table: "Auctions",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
