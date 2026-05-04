using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoadStallAPI.Migrations
{
    /// <inheritdoc />
    public partial class Third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "StockTake",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StockTake_StockId",
                table: "StockTake",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTake_UserId",
                table: "StockTake",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StockChange_StockId",
                table: "StockChange",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_StockChange_UserId",
                table: "StockChange",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockChange_Stock_StockId",
                table: "StockChange",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockChange_User_UserId",
                table: "StockChange",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTake_Stock_StockId",
                table: "StockTake",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTake_User_UserId",
                table: "StockTake",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockChange_Stock_StockId",
                table: "StockChange");

            migrationBuilder.DropForeignKey(
                name: "FK_StockChange_User_UserId",
                table: "StockChange");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTake_Stock_StockId",
                table: "StockTake");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTake_User_UserId",
                table: "StockTake");

            migrationBuilder.DropIndex(
                name: "IX_StockTake_StockId",
                table: "StockTake");

            migrationBuilder.DropIndex(
                name: "IX_StockTake_UserId",
                table: "StockTake");

            migrationBuilder.DropIndex(
                name: "IX_StockChange_StockId",
                table: "StockChange");

            migrationBuilder.DropIndex(
                name: "IX_StockChange_UserId",
                table: "StockChange");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StockTake");
        }
    }
}
