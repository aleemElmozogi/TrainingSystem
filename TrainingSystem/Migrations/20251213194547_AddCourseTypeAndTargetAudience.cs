using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTypeAndTargetAudience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseType",
                table: "Courses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetAudience",
                table: "Courses",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TargetAudience",
                table: "Courses");
        }
    }
}
