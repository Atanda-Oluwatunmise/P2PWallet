using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class DbContextChangee : Migration
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
                column: "UserId",
                unique: true);
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
                column: "UserId");
        }
    }
}
