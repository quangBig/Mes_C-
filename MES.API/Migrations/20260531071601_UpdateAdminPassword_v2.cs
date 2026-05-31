using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$s0Xy/Hzdb4tj7W6FD/A6HupJbei42bMO2OlUADSww56PIPsqZTZIm");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "");
        }
    }
}
