using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class ImageRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageDetails_ImageId",
                table: "ImageDetails");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDetails_ImageId",
                table: "ImageDetails",
                column: "ImageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImageDetails_ImageId",
                table: "ImageDetails");

            migrationBuilder.CreateIndex(
                name: "IX_ImageDetails_ImageId",
                table: "ImageDetails",
                column: "ImageId");
        }
    }
}
