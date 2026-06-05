using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class FixMachineForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_ProductionLines_ProductionLineLineId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_ProductionLineLineId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ProductionLineLineId",
                table: "Machines");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_LineId",
                table: "Machines",
                column: "LineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_ProductionLines_LineId",
                table: "Machines",
                column: "LineId",
                principalTable: "ProductionLines",
                principalColumn: "LineId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_ProductionLines_LineId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_LineId",
                table: "Machines");

            migrationBuilder.AddColumn<int>(
                name: "ProductionLineLineId",
                table: "Machines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_ProductionLineLineId",
                table: "Machines",
                column: "ProductionLineLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_ProductionLines_ProductionLineLineId",
                table: "Machines",
                column: "ProductionLineLineId",
                principalTable: "ProductionLines",
                principalColumn: "LineId");
        }
    }
}
