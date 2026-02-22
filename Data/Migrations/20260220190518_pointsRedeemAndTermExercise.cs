using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class pointsRedeemAndTermExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "points_redeem",
                table: "exercise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "term_at",
                table: "exercise",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "points_redeem",
                table: "exercise");

            migrationBuilder.DropColumn(
                name: "term_at",
                table: "exercise");
        }
    }
}
