using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSerialNumberIdToBigInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Drop Primary Key constraint trước
            migrationBuilder.Sql("ALTER TABLE [SerialNumbers] DROP CONSTRAINT [PK_SerialNumbers];");

            // Bước 2: Đổi kiểu cột từ int → bigint
            migrationBuilder.Sql("ALTER TABLE [SerialNumbers] ALTER COLUMN [SerialNumberId] bigint NOT NULL;");

            // Bước 3: Tạo lại Primary Key
            migrationBuilder.Sql("ALTER TABLE [SerialNumbers] ADD CONSTRAINT [PK_SerialNumbers] PRIMARY KEY ([SerialNumberId]);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: bigint → int
            migrationBuilder.Sql("ALTER TABLE [SerialNumbers] DROP CONSTRAINT [PK_SerialNumbers];");
            migrationBuilder.Sql("ALTER TABLE [SerialNumbers] ALTER COLUMN [SerialNumberId] int NOT NULL;");
            migrationBuilder.Sql("ALTER TABLE [SerialNumbers] ADD CONSTRAINT [PK_SerialNumbers] PRIMARY KEY ([SerialNumberId]);");
        }
    }
}
