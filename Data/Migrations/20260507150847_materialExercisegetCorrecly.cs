using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class materialExercisegetCorrecly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "material",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_materials_reference_exercises_exercise_id",
                table: "materials_reference_exercises",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_materials_reference_exercises_material_id",
                table: "materials_reference_exercises",
                column: "material_id");

            migrationBuilder.AddForeignKey(
                name: "FK_materials_reference_exercises_exercise_exercise_id",
                table: "materials_reference_exercises",
                column: "exercise_id",
                principalTable: "exercise",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_materials_reference_exercises_material_material_id",
                table: "materials_reference_exercises",
                column: "material_id",
                principalTable: "material",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_materials_reference_exercises_exercise_exercise_id",
                table: "materials_reference_exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_materials_reference_exercises_material_material_id",
                table: "materials_reference_exercises");

            migrationBuilder.DropIndex(
                name: "IX_materials_reference_exercises_exercise_id",
                table: "materials_reference_exercises");

            migrationBuilder.DropIndex(
                name: "IX_materials_reference_exercises_material_id",
                table: "materials_reference_exercises");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                table: "material",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
