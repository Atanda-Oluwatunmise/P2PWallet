using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class UpdateandCreateGLTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "CurrenciesWallets",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GLTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GlId = table.Column<int>(type: "int", nullable: false),
                    GlAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GLTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GLTransactions_GLAccounts_GlId",
                        column: x => x.GlId,
                        principalTable: "GLAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GLTransactions_GlId",
                table: "GLTransactions",
                column: "GlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GLTransactions");

            migrationBuilder.DropColumn(
                name: "Rate",
                table: "CurrenciesWallets");
        }
    }
}
