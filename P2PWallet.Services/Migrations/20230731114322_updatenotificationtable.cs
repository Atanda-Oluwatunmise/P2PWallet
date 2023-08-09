using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class updatenotificationtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "UserNotificationId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserNotificationId");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Notifications",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverUserId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SenderUserId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserNotificationId",
                table: "Notifications",
                column: "UserNotificationId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserNotificationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReceiverUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserNotificationId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserNotificationId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
