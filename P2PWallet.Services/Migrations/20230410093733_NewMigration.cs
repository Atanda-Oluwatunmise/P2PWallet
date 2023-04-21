using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "SecurityQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "SecurityQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_Users_UserId",
                table: "SecurityQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SecurityQuestions_UserId",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SecurityQuestions");
        }
    }
}
