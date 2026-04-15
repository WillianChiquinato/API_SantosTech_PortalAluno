using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class newNameGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_students_goals_rewards_goal_id",
                table: "goals_students");

            migrationBuilder.RenameColumn(
                name: "goal_id",
                table: "goals_students",
                newName: "goal_reward_id");

            migrationBuilder.RenameIndex(
                name: "IX_goals_students_goal_id",
                table: "goals_students",
                newName: "IX_goals_students_goal_reward_id");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_students_goals_rewards_goal_reward_id",
                table: "goals_students",
                column: "goal_reward_id",
                principalTable: "goals_rewards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_goals_students_goals_rewards_goal_reward_id",
                table: "goals_students");

            migrationBuilder.RenameColumn(
                name: "goal_reward_id",
                table: "goals_students",
                newName: "goal_id");

            migrationBuilder.RenameIndex(
                name: "IX_goals_students_goal_reward_id",
                table: "goals_students",
                newName: "IX_goals_students_goal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_goals_students_goals_rewards_goal_id",
                table: "goals_students",
                column: "goal_id",
                principalTable: "goals_rewards",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
