using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class dateTargetToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "container_date_target",
                table: "container_tasks");

            migrationBuilder.AddColumn<int>(
                name: "container_date_target_int",
                table: "container_tasks",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "container_date_target_int",
                table: "container_tasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "container_date_target",
                table: "container_tasks",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
