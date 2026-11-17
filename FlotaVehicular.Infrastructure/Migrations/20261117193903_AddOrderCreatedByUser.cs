using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlotaVehicular.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderCreatedByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "MaintenanceOrder",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceOrder_CreatedByUserId",
                table: "MaintenanceOrder",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceOrder_User_CreatedByUserId",
                table: "MaintenanceOrder",
                column: "CreatedByUserId",
                principalTable: "User",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceOrder_User_CreatedByUserId",
                table: "MaintenanceOrder");

            migrationBuilder.DropIndex(
                name: "IX_MaintenanceOrder_CreatedByUserId",
                table: "MaintenanceOrder");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MaintenanceOrder");
        }
    }
}
