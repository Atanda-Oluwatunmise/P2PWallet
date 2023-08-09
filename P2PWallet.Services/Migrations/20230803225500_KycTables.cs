using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class KycTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KycDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KycDocumentUploads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KycDocumentUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KycDocumentUploads_KycDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "KycDocuments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KycDocumentUploads_DocumentId",
                table: "KycDocumentUploads",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KycDocumentUploads");

            migrationBuilder.DropTable(
                name: "KycDocuments");
        }
    }
}
