using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cập nhật password của admin thành BCrypt hash đúng định dạng
            // Hash của chuỗi "admin" với work factor 11
            migrationBuilder.Sql(
                "UPDATE Users SET PasswordHash = '$2a$11$N6t7OynLp0/PJd9S7pYbQ.K3sEIy7A3Gf/bUEgqJNk3P/PVFhGe6' WHERE UserName = 'admin'"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Khôi phục lại plain text (chỉ để rollback)
            migrationBuilder.Sql(
                "UPDATE Users SET PasswordHash = 'admin' WHERE UserName = 'admin'"
            );
        }
    }
}
