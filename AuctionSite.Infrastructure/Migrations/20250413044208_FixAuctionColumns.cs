using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionSite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAuctionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Users_HighestBidderId",
                table: "Auctions");

            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Users_SellerId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_HighestBidderId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_SellerId",
                table: "Auctions");

            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Auctions",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "HighestBidderUsername",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SellerUsername",
                table: "Auctions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Auctions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_UserId",
                table: "Auctions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Users_UserId",
                table: "Auctions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auctions_Users_UserId",
                table: "Auctions");

            migrationBuilder.DropIndex(
                name: "IX_Auctions_UserId",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "HighestBidderUsername",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "SellerUsername",
                table: "Auctions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Auctions");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Auctions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_HighestBidderId",
                table: "Auctions",
                column: "HighestBidderId");

            migrationBuilder.CreateIndex(
                name: "IX_Auctions_SellerId",
                table: "Auctions",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Users_HighestBidderId",
                table: "Auctions",
                column: "HighestBidderId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Auctions_Users_SellerId",
                table: "Auctions",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
