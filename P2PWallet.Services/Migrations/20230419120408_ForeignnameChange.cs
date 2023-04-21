using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class ForeignnameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageDetails_Users_ImageId",
                table: "ImageDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Pin_Users_PinId",
                table: "Pin");

            migrationBuilder.DropForeignKey(
                name: "FK_ResetPasswords_Users_EmailId",
                table: "ResetPasswords");

            migrationBuilder.DropForeignKey(
                name: "FK_ResetPins_Users_EmailId",
                table: "ResetPins");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_Users_SecurityId",
                table: "SecurityQuestions");

            migrationBuilder.RenameColumn(
                name: "SecurityId",
                table: "SecurityQuestions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityQuestions_SecurityId",
                table: "SecurityQuestions",
                newName: "IX_SecurityQuestions_UserId");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "ResetPins",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPins_EmailId",
                table: "ResetPins",
                newName: "IX_ResetPins_UserId");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "ResetPasswords",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPasswords_EmailId",
                table: "ResetPasswords",
                newName: "IX_ResetPasswords_UserId");

            migrationBuilder.RenameColumn(
                name: "PinId",
                table: "Pin",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Pin_PinId",
                table: "Pin",
                newName: "IX_Pin_UserId");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "ImageDetails",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageDetails_ImageId",
                table: "ImageDetails",
                newName: "IX_ImageDetails_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageDetails_Users_UserId",
                table: "ImageDetails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pin_Users_UserId",
                table: "Pin",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPasswords_Users_UserId",
                table: "ResetPasswords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPins_Users_UserId",
                table: "ResetPins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityQuestions_Users_UserId",
                table: "SecurityQuestions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageDetails_Users_UserId",
                table: "ImageDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Pin_Users_UserId",
                table: "Pin");

            migrationBuilder.DropForeignKey(
                name: "FK_ResetPasswords_Users_UserId",
                table: "ResetPasswords");

            migrationBuilder.DropForeignKey(
                name: "FK_ResetPins_Users_UserId",
                table: "ResetPins");

            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_Users_UserId",
                table: "SecurityQuestions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SecurityQuestions",
                newName: "SecurityId");

            migrationBuilder.RenameIndex(
                name: "IX_SecurityQuestions_UserId",
                table: "SecurityQuestions",
                newName: "IX_SecurityQuestions_SecurityId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ResetPins",
                newName: "EmailId");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPins_UserId",
                table: "ResetPins",
                newName: "IX_ResetPins_EmailId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ResetPasswords",
                newName: "EmailId");

            migrationBuilder.RenameIndex(
                name: "IX_ResetPasswords_UserId",
                table: "ResetPasswords",
                newName: "IX_ResetPasswords_EmailId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Pin",
                newName: "PinId");

            migrationBuilder.RenameIndex(
                name: "IX_Pin_UserId",
                table: "Pin",
                newName: "IX_Pin_PinId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ImageDetails",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_ImageDetails_UserId",
                table: "ImageDetails",
                newName: "IX_ImageDetails_ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageDetails_Users_ImageId",
                table: "ImageDetails",
                column: "ImageId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pin_Users_PinId",
                table: "Pin",
                column: "PinId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPasswords_Users_EmailId",
                table: "ResetPasswords",
                column: "EmailId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ResetPins_Users_EmailId",
                table: "ResetPins",
                column: "EmailId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityQuestions_Users_SecurityId",
                table: "SecurityQuestions",
                column: "SecurityId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
