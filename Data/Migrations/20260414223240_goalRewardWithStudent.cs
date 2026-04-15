using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class goalRewardWithStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_students_goals_goal_id",
                table: "goals_students");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_students_goals_rewards_goal_id",
                table: "goals_students",
                column: "goal_id",
                principalTable: "goals_rewards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_students_goals_rewards_goal_id",
                table: "goals_students");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_students_goals_goal_id",
                table: "goals_students",
                column: "goal_id",
                principalTable: "goals",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
