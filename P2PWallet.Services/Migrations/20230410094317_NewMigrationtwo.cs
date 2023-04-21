 using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class NewMigrationtwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_Users_UserId",
                table: "SecurityQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SecurityQuestions_UserId",
                table: "SecurityQuestions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SecurityQuestions",
                newName: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityQuestions_SecurityId",
                table: "SecurityQuestions",
                column: "SecurityId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityQuestions_Users_SecurityId",
                table: "SecurityQuestions",
                column: "SecurityId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_Users_SecurityId",
                table: "SecurityQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SecurityQuestions_SecurityId",
                table: "SecurityQuestions");

            migrationBuilder.RenameColumn(
                name: "SecurityId",
                table: "SecurityQuestions",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityQuestions_UserId",
                table: "SecurityQuestions",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityQuestions_Users_UserId",
                table: "SecurityQuestions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
