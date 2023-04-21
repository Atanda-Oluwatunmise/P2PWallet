using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class DbContextChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageDetails_UserId",
                table: "ImageDetails");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDetails_UserId",
                table: "ImageDetails",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageDetails_UserId",
                table: "ImageDetails");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDetails_UserId",
                table: "ImageDetails",
                column: "UserId",
                unique: true);
        }
    }
}
