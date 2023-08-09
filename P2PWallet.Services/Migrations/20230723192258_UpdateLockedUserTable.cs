using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLockedUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "LockedUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "LockedUsers");
        }
    }
}
