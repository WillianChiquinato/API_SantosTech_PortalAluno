using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class tableDailyTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    exercise_id = table.Column<int>(type: "integer", nullable: false),
                    phase_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_tasks_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_daily_tasks_phase_phase_id",
                        column: x => x.phase_id,
                        principalTable: "phase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_tasks_exercise_id",
                table: "daily_tasks",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_tasks_phase_id",
                table: "daily_tasks",
                column: "phase_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_tasks");
        }
    }
}
