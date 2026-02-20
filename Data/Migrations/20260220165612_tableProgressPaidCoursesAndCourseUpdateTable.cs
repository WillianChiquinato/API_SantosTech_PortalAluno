using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class tableProgressPaidCoursesAndCourseUpdateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "duration_hours",
                table: "course",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "level_difficulty",
                table: "course",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "paid_focus",
                table: "course",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "course",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "progress_paid_courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    progress_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    last_accessed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_progress_paid_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_progress_paid_courses_course_course_id",
                        column: x => x.course_id,
                        principalTable: "course",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_progress_paid_courses_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_progress_paid_courses_course_id",
                table: "progress_paid_courses",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_progress_paid_courses_user_id",
                table: "progress_paid_courses",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "progress_paid_courses");

            migrationBuilder.DropColumn(
                name: "duration_hours",
                table: "course");

            migrationBuilder.DropColumn(
                name: "level_difficulty",
                table: "course");

            migrationBuilder.DropColumn(
                name: "paid_focus",
                table: "course");

            migrationBuilder.DropColumn(
                name: "price",
                table: "course");
        }
    }
}
