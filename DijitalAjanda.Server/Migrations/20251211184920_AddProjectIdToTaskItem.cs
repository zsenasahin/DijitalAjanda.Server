using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectIdToTaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if TaskItems table exists before adding column
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaskItems')
                BEGIN
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaskItems' AND COLUMN_NAME = 'ProjectId')
                    BEGIN
                        ALTER TABLE TaskItems ADD ProjectId int NULL;
                    END
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaskItems')
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItems_ProjectId' AND object_id = OBJECT_ID('TaskItems'))
                    BEGIN
                        CREATE INDEX IX_TaskItems_ProjectId ON TaskItems(ProjectId);
                    END
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TaskItems')
                AND EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Projects')
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TaskItems_Projects_ProjectId')
                    BEGIN
                        ALTER TABLE TaskItems
                        ADD CONSTRAINT FK_TaskItems_Projects_ProjectId
                        FOREIGN KEY (ProjectId) REFERENCES Projects(Id);
                    END
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TaskItems_Projects_ProjectId')
                BEGIN
                    ALTER TABLE TaskItems DROP CONSTRAINT FK_TaskItems_Projects_ProjectId;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskItems_ProjectId' AND object_id = OBJECT_ID('TaskItems'))
                BEGIN
                    DROP INDEX IX_TaskItems_ProjectId ON TaskItems;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TaskItems' AND COLUMN_NAME = 'ProjectId')
                BEGIN
                    ALTER TABLE TaskItems DROP COLUMN ProjectId;
                END
            ");
        }
    }
}
