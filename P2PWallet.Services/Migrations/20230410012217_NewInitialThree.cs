using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class NewInitialThree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SecurityQuestions_Users_UserSecurityId",
                table: "SecurityQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SecurityQuestions_UserSecurityId",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "Answer",
                table: "SecurityQuestions");

            migrationBuilder.DropColumn(
                name: "UserSecurityId",
                table: "SecurityQuestions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "SecurityQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserSecurityId",
                table: "SecurityQuestions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityQuestions_UserSecurityId",
                table: "SecurityQuestions",
                column: "UserSecurityId");

            migrationBuilder.AddForeignKey(
                name: "FK_SecurityQuestions_Users_UserSecurityId",
                table: "SecurityQuestions",
                column: "UserSecurityId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
