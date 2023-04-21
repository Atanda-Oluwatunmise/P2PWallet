using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class Imageforeingname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageDetails_Users_UserId",
                table: "ImageDetails");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ImageDetails",
                newName: "ImageUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageDetails_UserId",
                table: "ImageDetails",
                newName: "IX_ImageDetails_ImageUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageDetails_Users_ImageUserId",
                table: "ImageDetails",
                column: "ImageUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageDetails_Users_ImageUserId",
                table: "ImageDetails");

            migrationBuilder.RenameColumn(
                name: "ImageUserId",
                table: "ImageDetails",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageDetails_ImageUserId",
                table: "ImageDetails",
                newName: "IX_ImageDetails_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageDetails_Users_UserId",
                table: "ImageDetails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
