using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlotaVehicular.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechniciansSpecialtiesOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Specialty",
                columns: table => new
                {
                    IdSpecialty = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialty", x => x.IdSpecialty);
                });

            migrationBuilder.CreateTable(
                name: "Technician",
                columns: table => new
                {
                    IdTechnician = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IdentificationNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technician", x => x.IdTechnician);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceOrder",
                columns: table => new
                {
                    IdMaintenanceOrder = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdVehicle = table.Column<int>(type: "int", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdRequiredSpecialty = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(SYSUTCDATETIME())"),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceOrder", x => x.IdMaintenanceOrder);
                    table.ForeignKey(
                        name: "FK_MaintenanceOrder_Specialty_IdRequiredSpecialty",
                        column: x => x.IdRequiredSpecialty,
                        principalTable: "Specialty",
                        principalColumn: "IdSpecialty",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceOrder_Vehicle_IdVehicle",
                        column: x => x.IdVehicle,
                        principalTable: "Vehicle",
                        principalColumn: "IdVehicle",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TechnicianSpecialty",
                columns: table => new
                {
                    IdTechnician = table.Column<int>(type: "int", nullable: false),
                    IdSpecialty = table.Column<int>(type: "int", nullable: false),
                    CertificationLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SinceDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianSpecialty", x => new { x.IdTechnician, x.IdSpecialty });
                    table.ForeignKey(
                        name: "FK_TechnicianSpecialty_Specialty_IdSpecialty",
                        column: x => x.IdSpecialty,
                        principalTable: "Specialty",
                        principalColumn: "IdSpecialty",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TechnicianSpecialty_Technician_IdTechnician",
                        column: x => x.IdTechnician,
                        principalTable: "Technician",
                        principalColumn: "IdTechnician",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrder_IdRequiredSpecialty",
                table: "MaintenanceOrder",
                column: "IdRequiredSpecialty");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrder_IdVehicle",
                table: "MaintenanceOrder",
                column: "IdVehicle");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrder_OrderCode",
                table: "MaintenanceOrder",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrder_Status",
                table: "MaintenanceOrder",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Specialty_Name",
                table: "Specialty",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technician_IdentificationNumber",
                table: "Technician",
                column: "IdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Technician_IsActive",
                table: "Technician",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Technician_IsAvailable",
                table: "Technician",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianSpecialty_IdSpecialty",
                table: "TechnicianSpecialty",
                column: "IdSpecialty");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceOrder");

            migrationBuilder.DropTable(
                name: "TechnicianSpecialty");

            migrationBuilder.DropTable(
                name: "Specialty");

            migrationBuilder.DropTable(
                name: "Technician");
        }
    }
}
