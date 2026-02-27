using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class modifyTableUserFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_flow_exercise_id",
                table: "user_exercise_flow",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_flow_phase_id",
                table: "user_exercise_flow",
                column: "phase_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_flow_user_id",
                table: "user_exercise_flow",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_exercise_flow_exercise_exercise_id",
                table: "user_exercise_flow",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_exercise_flow_phase_phase_id",
                table: "user_exercise_flow",
                column: "phase_id",
                principalTable: "phase",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_exercise_flow_user_user_id",
                table: "user_exercise_flow",
                column: "user_id",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_exercise_flow_exercise_exercise_id",
                table: "user_exercise_flow");

            migrationBuilder.DropForeignKey(
                name: "FK_user_exercise_flow_phase_phase_id",
                table: "user_exercise_flow");

            migrationBuilder.DropForeignKey(
                name: "FK_user_exercise_flow_user_user_id",
                table: "user_exercise_flow");

            migrationBuilder.DropIndex(
                name: "IX_user_exercise_flow_exercise_id",
                table: "user_exercise_flow");

            migrationBuilder.DropIndex(
                name: "IX_user_exercise_flow_phase_id",
                table: "user_exercise_flow");

            migrationBuilder.DropIndex(
                name: "IX_user_exercise_flow_user_id",
                table: "user_exercise_flow");
        }
    }
}
