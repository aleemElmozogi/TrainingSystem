using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPresent",
                table: "AttendanceRecords",
                newName: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "AttendanceRecords",
                newName: "IsPresent");
        }
    }
}
