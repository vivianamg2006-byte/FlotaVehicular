using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlotaVehicular.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SparePart",
                columns: table => new
                {
                    IdSparePart = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    MinStock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SparePart", x => x.IdSparePart);
                    table.CheckConstraint("CK_SparePart_CurrentStock", "[CurrentStock] >= 0");
                    table.CheckConstraint("CK_SparePart_MinStock", "[MinStock] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    IdSupplier = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    GeneralConditions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.IdSupplier);
                });

            migrationBuilder.CreateTable(
                name: "VehicleCategory",
                columns: table => new
                {
                    IdVehicleCategory = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreventiveMaintenanceIntervalKm = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCategory", x => x.IdVehicleCategory);
                });

            migrationBuilder.CreateTable(
                name: "SparePartSupplier",
                columns: table => new
                {
                    IdSparePart = table.Column<int>(type: "int", nullable: false),
                    IdSupplier = table.Column<int>(type: "int", nullable: false),
                    OfferedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedDeliveryDays = table.Column<int>(type: "int", nullable: false),
                    AssociatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(SYSUTCDATETIME())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SparePartSupplier", x => new { x.IdSparePart, x.IdSupplier });
                    table.ForeignKey(
                        name: "FK_SparePartSupplier_SparePart_IdSparePart",
                        column: x => x.IdSparePart,
                        principalTable: "SparePart",
                        principalColumn: "IdSparePart",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SparePartSupplier_Supplier_IdSupplier",
                        column: x => x.IdSupplier,
                        principalTable: "Supplier",
                        principalColumn: "IdSupplier",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    IdVehicle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "date", nullable: false),
                    ReferenceValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdVehicleCategory = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CurrentMileage = table.Column<int>(type: "int", nullable: false),
                    LastPreventiveMaintenanceMileage = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(SYSUTCDATETIME())"),
                    DeactivationDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.IdVehicle);
                    table.ForeignKey(
                        name: "FK_Vehicle_VehicleCategory_IdVehicleCategory",
                        column: x => x.IdVehicleCategory,
                        principalTable: "VehicleCategory",
                        principalColumn: "IdVehicleCategory",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VehicleImage",
                columns: table => new
                {
                    IdVehicleImage = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVehicle = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    PublicUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(SYSUTCDATETIME())"),
                    IsMain = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleImage", x => x.IdVehicleImage);
                    table.ForeignKey(
                        name: "FK_VehicleImage_Vehicle_IdVehicle",
                        column: x => x.IdVehicle,
                        principalTable: "Vehicle",
                        principalColumn: "IdVehicle",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SparePart_Code",
                table: "SparePart",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SparePartSupplier_IdSupplier",
                table: "SparePartSupplier",
                column: "IdSupplier");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_Name",
                table: "Supplier",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_Status",
                table: "Supplier",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_Code",
                table: "Vehicle",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_IdVehicleCategory",
                table: "Vehicle",
                column: "IdVehicleCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_LicensePlate",
                table: "Vehicle",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicle_Status",
                table: "Vehicle",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleCategory_Name",
                table: "VehicleCategory",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleImage_IdVehicle",
                table: "VehicleImage",
                column: "IdVehicle");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleImage_IdVehicle_FileName",
                table: "VehicleImage",
                columns: new[] { "IdVehicle", "FileName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SparePartSupplier");

            migrationBuilder.DropTable(
                name: "VehicleImage");

            migrationBuilder.DropTable(
                name: "SparePart");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "Vehicle");

            migrationBuilder.DropTable(
                name: "VehicleCategory");
        }
    }
}
