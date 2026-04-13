using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyGoalPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE goals
                ALTER COLUMN type DROP DEFAULT;

                ALTER TABLE goals
                ALTER COLUMN type TYPE integer
                USING CASE
                    WHEN type IS NULL OR btrim(type) = '' THEN 0
                    WHEN type ~ '^[0-9]+$' THEN type::integer
                    WHEN lower(btrim(type)) = 'coursecompletion' THEN 1
                    WHEN lower(btrim(type)) = 'phasecompletion' THEN 2
                    WHEN lower(btrim(type)) = 'taskquantity' THEN 3
                    WHEN lower(btrim(type)) = 'timespent' THEN 4
                    WHEN lower(btrim(type)) = 'custom' THEN 5
                    ELSE 0
                END;

                ALTER TABLE goals
                ALTER COLUMN type SET DEFAULT 0;

                ALTER TABLE goals
                ALTER COLUMN type SET NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "goals",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
