using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectPriorityStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix Project Priority and Status columns - convert from int to string if needed
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'Priority' AND DATA_TYPE = 'int')
                BEGIN
                    ALTER TABLE Projects ALTER COLUMN Priority nvarchar(max);
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'Status' AND DATA_TYPE = 'int')
                BEGIN
                    ALTER TABLE Projects ALTER COLUMN Status nvarchar(max);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert changes if needed
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'Priority' AND DATA_TYPE = 'nvarchar')
                BEGIN
                    ALTER TABLE Projects ALTER COLUMN Priority int;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Projects' AND COLUMN_NAME = 'Status' AND DATA_TYPE = 'nvarchar')
                BEGIN
                    ALTER TABLE Projects ALTER COLUMN Status int;
                END
            ");
        }
    }
}
