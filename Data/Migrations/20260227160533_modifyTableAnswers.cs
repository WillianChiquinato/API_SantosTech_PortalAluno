using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class modifyTableAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "user_exercise_flow_id",
                table: "answer",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_answer_user_exercise_flow_id",
                table: "answer",
                column: "user_exercise_flow_id");

            migrationBuilder.AddForeignKey(
                name: "FK_answer_user_exercise_flow_user_exercise_flow_id",
                table: "answer",
                column: "user_exercise_flow_id",
                principalTable: "user_exercise_flow",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answer_user_exercise_flow_user_exercise_flow_id",
                table: "answer");

            migrationBuilder.DropIndex(
                name: "IX_answer_user_exercise_flow_id",
                table: "answer");

            migrationBuilder.DropColumn(
                name: "user_exercise_flow_id",
                table: "answer");
        }
    }
}
