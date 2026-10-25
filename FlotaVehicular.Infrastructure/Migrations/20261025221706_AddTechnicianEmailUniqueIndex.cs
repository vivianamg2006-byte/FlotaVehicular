using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlotaVehicular.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianEmailUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Technician_Email",
                table: "Technician",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Technician_Email",
                table: "Technician");
        }
    }
}
