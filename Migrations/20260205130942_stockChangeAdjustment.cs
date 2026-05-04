using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadStallAPI.Migrations
{
    /// <inheritdoc />
    public partial class stockChangeAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Adjustment",
                table: "StockChange",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adjustment",
                table: "StockChange");
        }
    }
}
