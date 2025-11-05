using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DijitalAjanda.Server.Migrations
{
    /// <inheritdoc />
    public partial class ChangeBookStatusToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Status kolonunu int'den string'e çevir
            migrationBuilder.Sql(@"
                -- Geçici kolon oluştur
                ALTER TABLE Books ADD StatusTemp NVARCHAR(MAX) NULL;
                
                -- Mevcut int değerleri string'e çevir
                UPDATE Books
                SET StatusTemp = 
                    CASE 
                        WHEN Status = 0 THEN 'ToRead'
                        WHEN Status = 1 THEN 'CurrentlyReading'
                        WHEN Status = 2 THEN 'Completed'
                        WHEN Status = 3 THEN 'Abandoned'
                        ELSE 'ToRead'
                    END;
                
                -- Eski kolonu sil
                ALTER TABLE Books DROP COLUMN Status;
                
                -- Geçici kolonu asıl kolona çevir
                EXEC sp_rename 'Books.StatusTemp', 'Status', 'COLUMN';
                
                -- NOT NULL yap
                ALTER TABLE Books ALTER COLUMN Status NVARCHAR(MAX) NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Status kolonunu string'den int'e geri çevir
            migrationBuilder.Sql(@"
                -- Geçici kolon oluştur
                ALTER TABLE Books ADD StatusTemp INT NULL;
                
                -- Mevcut string değerleri int'e çevir
                UPDATE Books
                SET StatusTemp = 
                    CASE 
                        WHEN Status = 'ToRead' THEN 0
                        WHEN Status = 'CurrentlyReading' THEN 1
                        WHEN Status = 'Completed' THEN 2
                        WHEN Status = 'Abandoned' THEN 3
                        ELSE 0
                    END;
                
                -- Eski kolonu sil
                ALTER TABLE Books DROP COLUMN Status;
                
                -- Geçici kolonu asıl kolona çevir
                EXEC sp_rename 'Books.StatusTemp', 'Status', 'COLUMN';
                
                -- NOT NULL yap
                ALTER TABLE Books ALTER COLUMN Status INT NOT NULL;
            ");
        }
    }
}

