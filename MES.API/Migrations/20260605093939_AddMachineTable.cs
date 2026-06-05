using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    MachineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineId = table.Column<int>(type: "int", nullable: false),
                    MachineCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductionLineLineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.MachineId);
                    table.ForeignKey(
                        name: "FK_Machines_ProductionLines_ProductionLineLineId",
                        column: x => x.ProductionLineLineId,
                        principalTable: "ProductionLines",
                        principalColumn: "LineId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Machines_ProductionLineLineId",
                table: "Machines",
                column: "ProductionLineLineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Machines");
        }
    }
}
