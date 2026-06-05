using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToWorkOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedBy",
                table: "WorkOrders",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_CreatedBy",
                table: "WorkOrders",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_CreatedBy",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CreatedBy",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkOrders");
        }
    }
}
