using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class MakeTaskItemDescriptionNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if TaskItems table exists before modifying
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaskItems')
                BEGIN
                    DECLARE @var sysname;
                    SELECT @var = [d].[name]
                    FROM [sys].[default_constraints] [d]
                    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TaskItems]') AND [c].[name] = N'Description');
                    IF @var IS NOT NULL EXEC(N'ALTER TABLE [TaskItems] DROP CONSTRAINT [' + @var + '];');
                    ALTER TABLE [TaskItems] ALTER COLUMN [Description] nvarchar(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaskItems')
                BEGIN
                    ALTER TABLE [TaskItems] ALTER COLUMN [Description] nvarchar(max) NOT NULL;
                END
            ");
        }
    }
}
