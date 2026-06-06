using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MES.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionTrackingV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WorkOrders",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "SerialCode",
                table: "SerialNumbers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Defects",
                columns: table => new
                {
                    DefectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefectCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefectName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Defects", x => x.DefectId);
                });

            migrationBuilder.CreateTable(
                name: "ProductionTrackings",
                columns: table => new
                {
                    ProductionTrackingId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumberId = table.Column<long>(type: "bigint", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    OperatorId = table.Column<int>(type: "int", nullable: true),
                    ProcessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefectId = table.Column<int>(type: "int", nullable: true),
                    ScanSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionTrackings", x => x.ProductionTrackingId);
                    table.ForeignKey(
                        name: "FK_ProductionTrackings_Defects_DefectId",
                        column: x => x.DefectId,
                        principalTable: "Defects",
                        principalColumn: "DefectId");
                    table.ForeignKey(
                        name: "FK_ProductionTrackings_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "MachineId");
                    table.ForeignKey(
                        name: "FK_ProductionTrackings_SerialNumbers_SerialNumberId",
                        column: x => x.SerialNumberId,
                        principalTable: "SerialNumbers",
                        principalColumn: "SerialNumberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionTrackings_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionTrackings_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId");
                });

            migrationBuilder.InsertData(
                table: "Defects",
                columns: new[] { "DefectId", "DefectCode", "DefectName" },
                values: new object[,]
                {
                    { 1, "DF001", "Missing Component" },
                    { 2, "DF002", "Wrong Part" },
                    { 3, "DF003", "Solder Bridge" },
                    { 4, "DF004", "Tombstone" },
                    { 5, "DF005", "Insufficient Solder" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_SerialCode",
                table: "SerialNumbers",
                column: "SerialCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTrackings_DefectId",
                table: "ProductionTrackings",
                column: "DefectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTrackings_MachineId",
                table: "ProductionTrackings",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTrackings_OperatorId",
                table: "ProductionTrackings",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTrackings_SerialNumberId",
                table: "ProductionTrackings",
                column: "SerialNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionTrackings_WorkOrderId",
                table: "ProductionTrackings",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionTrackings");

            migrationBuilder.DropTable(
                name: "Defects");

            migrationBuilder.DropIndex(
                name: "IX_SerialNumbers_SerialCode",
                table: "SerialNumbers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<string>(
                name: "SerialCode",
                table: "SerialNumbers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
