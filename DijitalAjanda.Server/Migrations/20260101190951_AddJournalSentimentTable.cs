using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalSentimentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JournalSentiments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    SentimentLabel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentimentScore = table.Column<float>(type: "real", nullable: false),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalSentiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalSentiments_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JournalSentiments_JournalEntryId",
                table: "JournalSentiments",
                column: "JournalEntryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalSentiments");
        }
    }
}
