using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesAndUpdatedAtToDailyTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if columns exist before adding
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DailyTasks' AND COLUMN_NAME = 'Notes')
                BEGIN
                    ALTER TABLE DailyTasks ADD Notes nvarchar(max) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DailyTasks' AND COLUMN_NAME = 'UpdatedAt')
                BEGIN
                    ALTER TABLE DailyTasks ADD UpdatedAt datetime2 NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DailyTasks' AND COLUMN_NAME = 'UpdatedAt')
                BEGIN
                    ALTER TABLE DailyTasks DROP COLUMN UpdatedAt;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DailyTasks' AND COLUMN_NAME = 'Notes')
                BEGIN
                    ALTER TABLE DailyTasks DROP COLUMN Notes;
                END
            ");
        }
    }
}
