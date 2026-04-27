using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class rankingSystemAwards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ranking_events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_name = table.Column<string>(type: "text", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranking_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ranking_awards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    award_name = table.Column<string>(type: "text", nullable: false),
                    award_position_ranking = table.Column<int>(type: "integer", nullable: false),
                    award_description = table.Column<string>(type: "text", nullable: false),
                    award_picture_url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranking_awards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ranking_awards_ranking_events_event_id",
                        column: x => x.event_id,
                        principalTable: "ranking_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ranking_history",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    award_id = table.Column<int>(type: "integer", nullable: false),
                    ranking_position = table.Column<int>(type: "integer", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranking_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ranking_history_ranking_awards_award_id",
                        column: x => x.award_id,
                        principalTable: "ranking_awards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ranking_history_ranking_events_event_id",
                        column: x => x.event_id,
                        principalTable: "ranking_events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ranking_history_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ranking_awards_event_id",
                table: "ranking_awards",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_ranking_history_award_id",
                table: "ranking_history",
                column: "award_id");

            migrationBuilder.CreateIndex(
                name: "IX_ranking_history_event_id",
                table: "ranking_history",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_ranking_history_user_id",
                table: "ranking_history",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ranking_history");

            migrationBuilder.DropTable(
                name: "ranking_awards");

            migrationBuilder.DropTable(
                name: "ranking_events");
        }
    }
}
