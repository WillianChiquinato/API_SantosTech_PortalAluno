using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class materialsRefacotyrTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_material_class_class_id",
                table: "material");

            migrationBuilder.RenameColumn(
                name: "class_id",
                table: "material",
                newName: "course_id");

            migrationBuilder.RenameIndex(
                name: "IX_material_class_id",
                table: "material",
                newName: "IX_material_course_id");

            migrationBuilder.AddForeignKey(
                name: "FK_material_course_course_id",
                table: "material",
                column: "course_id",
                principalTable: "course",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_material_course_course_id",
                table: "material");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "material",
                newName: "class_id");

            migrationBuilder.RenameIndex(
                name: "IX_material_course_id",
                table: "material",
                newName: "IX_material_class_id");

            migrationBuilder.AddForeignKey(
                name: "FK_material_class_class_id",
                table: "material",
                column: "class_id",
                principalTable: "class",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
