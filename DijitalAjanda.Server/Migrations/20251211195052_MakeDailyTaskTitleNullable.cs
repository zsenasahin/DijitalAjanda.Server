using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class MakeDailyTaskTitleNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if DailyTasks table exists and Title column exists, then make it nullable
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DailyTasks')
                BEGIN
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DailyTasks' AND COLUMN_NAME = 'Title')
                    BEGIN
                        DECLARE @var sysname;
                        SELECT @var = [d].[name]
                        FROM [sys].[default_constraints] [d]
                        INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                        WHERE ([d].[parent_object_id] = OBJECT_ID(N'[DailyTasks]') AND [c].[name] = N'Title');
                        IF @var IS NOT NULL EXEC(N'ALTER TABLE [DailyTasks] DROP CONSTRAINT [' + @var + '];');
                        ALTER TABLE [DailyTasks] ALTER COLUMN [Title] nvarchar(max) NULL;
                    END
                    ELSE
                    BEGIN
                        ALTER TABLE [DailyTasks] ADD [Title] nvarchar(max) NULL;
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DailyTasks')
                BEGIN
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DailyTasks' AND COLUMN_NAME = 'Title')
                    BEGIN
                        ALTER TABLE [DailyTasks] ALTER COLUMN [Title] nvarchar(max) NOT NULL;
                    END
                END
            ");
        }
    }
}
