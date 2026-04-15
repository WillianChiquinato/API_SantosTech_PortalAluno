using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class newGoalsVerifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "end_date_target",
                table: "goals_rewards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "reward_type",
                table: "goals_rewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date_target",
                table: "goals_rewards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_goals_rewards_course_id",
                table: "goals_rewards",
                column: "course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_rewards_course_course_id",
                table: "goals_rewards",
                column: "course_id",
                principalTable: "course",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_rewards_course_course_id",
                table: "goals_rewards");

            migrationBuilder.DropIndex(
                name: "IX_goals_rewards_course_id",
                table: "goals_rewards");

            migrationBuilder.DropColumn(
                name: "end_date_target",
                table: "goals_rewards");

            migrationBuilder.DropColumn(
                name: "reward_type",
                table: "goals_rewards");

            migrationBuilder.DropColumn(
                name: "start_date_target",
                table: "goals_rewards");
        }
    }
}
