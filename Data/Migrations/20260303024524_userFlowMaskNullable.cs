using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class userFlowMaskNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answer_user_exercise_flow_user_exercise_flow_id",
                table: "answer");

            migrationBuilder.AlterColumn<int>(
                name: "user_exercise_flow_id",
                table: "answer",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_answer_user_exercise_flow_user_exercise_flow_id",
                table: "answer",
                column: "user_exercise_flow_id",
                principalTable: "user_exercise_flow",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_answer_user_exercise_flow_user_exercise_flow_id",
                table: "answer");

            migrationBuilder.AlterColumn<int>(
                name: "user_exercise_flow_id",
                table: "answer",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_answer_user_exercise_flow_user_exercise_flow_id",
                table: "answer",
                column: "user_exercise_flow_id",
                principalTable: "user_exercise_flow",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
