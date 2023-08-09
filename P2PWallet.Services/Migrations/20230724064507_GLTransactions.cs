using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class GLTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "GLTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "GLTransactions");
        }
    }
}
