using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalChallengeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "final_challenge_event",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ranking_event_id = table.Column<int>(type: "integer", nullable: true),
                    module_id = table.Column<int>(type: "integer", nullable: false),
                    class_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    refresh_interval_seconds = table.Column<int>(type: "integer", nullable: false),
                    update_mode = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_final_challenge_event", x => x.id);
                    table.ForeignKey(
                        name: "FK_final_challenge_event_class_class_id",
                        column: x => x.class_id,
                        principalTable: "class",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_final_challenge_event_module_module_id",
                        column: x => x.module_id,
                        principalTable: "module",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_final_challenge_event_ranking_events_ranking_event_id",
                        column: x => x.ranking_event_id,
                        principalTable: "ranking_events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "final_challenge_clan",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    team_id = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    motto = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: false),
                    boat_name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_final_challenge_clan", x => x.id);
                    table.ForeignKey(
                        name: "FK_final_challenge_clan_final_challenge_event_event_id",
                        column: x => x.event_id,
                        principalTable: "final_challenge_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_final_challenge_clan_teams_challenger_team_id",
                        column: x => x.team_id,
                        principalTable: "teams_challenger",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "final_challenge_task",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    exercise_id = table.Column<int>(type: "integer", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    briefing = table.Column<string>(type: "text", nullable: true),
                    difficulty = table.Column<string>(type: "text", nullable: false),
                    score_weight = table.Column<int>(type: "integer", nullable: false),
                    deadline_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_final_challenge_task", x => x.id);
                    table.ForeignKey(
                        name: "FK_final_challenge_task_exercise_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercise",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_final_challenge_task_final_challenge_event_event_id",
                        column: x => x.event_id,
                        principalTable: "final_challenge_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "final_challenge_activity",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    clan_id = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    happened_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    metadata_json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_final_challenge_activity", x => x.id);
                    table.ForeignKey(
                        name: "FK_final_challenge_activity_final_challenge_clan_clan_id",
                        column: x => x.clan_id,
                        principalTable: "final_challenge_clan",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_final_challenge_activity_final_challenge_event_event_id",
                        column: x => x.event_id,
                        principalTable: "final_challenge_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "final_challenge_submission",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    clan_id = table.Column<int>(type: "integer", nullable: false),
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    answer = table.Column<string>(type: "text", nullable: false),
                    attachments_json = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    ai_score = table.Column<double>(type: "double precision", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    validated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_final_challenge_submission", x => x.id);
                    table.ForeignKey(
                        name: "FK_final_challenge_submission_final_challenge_clan_clan_id",
                        column: x => x.clan_id,
                        principalTable: "final_challenge_clan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_final_challenge_submission_final_challenge_event_event_id",
                        column: x => x.event_id,
                        principalTable: "final_challenge_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_final_challenge_submission_final_challenge_task_task_id",
                        column: x => x.task_id,
                        principalTable: "final_challenge_task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_final_challenge_submission_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_activity_clan_id",
                table: "final_challenge_activity",
                column: "clan_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_activity_event_id",
                table: "final_challenge_activity",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_clan_event_id",
                table: "final_challenge_clan",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_clan_team_id",
                table: "final_challenge_clan",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_event_class_id",
                table: "final_challenge_event",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_event_module_id",
                table: "final_challenge_event",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_event_ranking_event_id",
                table: "final_challenge_event",
                column: "ranking_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_submission_clan_id",
                table: "final_challenge_submission",
                column: "clan_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_submission_event_id",
                table: "final_challenge_submission",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_submission_task_id",
                table: "final_challenge_submission",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_submission_user_id",
                table: "final_challenge_submission",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_task_event_id",
                table: "final_challenge_task",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "IX_final_challenge_task_exercise_id",
                table: "final_challenge_task",
                column: "exercise_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "final_challenge_activity");

            migrationBuilder.DropTable(
                name: "final_challenge_submission");

            migrationBuilder.DropTable(
                name: "final_challenge_clan");

            migrationBuilder.DropTable(
                name: "final_challenge_task");

            migrationBuilder.DropTable(
                name: "final_challenge_event");
        }
    }
}
