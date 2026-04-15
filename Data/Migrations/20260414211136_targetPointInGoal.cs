using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class targetPointInGoal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "points_target",
                table: "goals_rewards",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "points_target",
                table: "goals_rewards");
        }
    }
}
