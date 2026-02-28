using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SamarStoneQwen.Migrations
{
    /// <inheritdoc />
    public partial class remvoeamount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAmountUSD",
                table: "PurchaseOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmountUSD",
                table: "PurchaseOrders",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
