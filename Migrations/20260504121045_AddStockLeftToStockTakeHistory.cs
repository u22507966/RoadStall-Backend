using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadStallAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLeftToStockTakeHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockLeft",
                table: "StockTakeHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockLeft",
                table: "StockTakeHistory");
        }
    }
}
