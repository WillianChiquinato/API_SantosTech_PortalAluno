using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace API_PortalSantosTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_template",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    title_template = table.Column<string>(type: "text", nullable: false),
                    message_template = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_actor_id = table.Column<string>(type: "text", nullable: true),
                    created_by_actor_name = table.Column<string>(type: "text", nullable: true),
                    created_by_actor_email = table.Column<string>(type: "text", nullable: true),
                    updated_by_actor_id = table.Column<string>(type: "text", nullable: true),
                    updated_by_actor_name = table.Column<string>(type: "text", nullable: true),
                    updated_by_actor_email = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_dispatch",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_template_id = table.Column<int>(type: "integer", nullable: false),
                    template_name = table.Column<string>(type: "text", nullable: false),
                    triggered_by_actor_id = table.Column<string>(type: "text", nullable: true),
                    triggered_by_actor_name = table.Column<string>(type: "text", nullable: true),
                    triggered_by_actor_email = table.Column<string>(type: "text", nullable: true),
                    filters_json = table.Column<string>(type: "text", nullable: false),
                    total_recipients = table.Column<int>(type: "integer", nullable: false),
                    failed_recipients = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_dispatch", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_dispatch_notification_template_notification_te~",
                        column: x => x.notification_template_id,
                        principalTable: "notification_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    notification_template_id = table.Column<int>(type: "integer", nullable: false),
                    notification_dispatch_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    metadata_json = table.Column<string>(type: "text", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_notification_dispatch_notification_dispatch_id",
                        column: x => x.notification_dispatch_id,
                        principalTable: "notification_dispatch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notification_notification_template_notification_template_id",
                        column: x => x.notification_template_id,
                        principalTable: "notification_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notification_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_dispatch_recipient",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_dispatch_id = table.Column<int>(type: "integer", nullable: false),
                    recipient_user_id = table.Column<int>(type: "integer", nullable: true),
                    recipient_name = table.Column<string>(type: "text", nullable: true),
                    recipient_email = table.Column<string>(type: "text", nullable: true),
                    class_name = table.Column<string>(type: "text", nullable: true),
                    course_name = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    failure_reason = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_dispatch_recipient", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_dispatch_recipient_notification_dispatch_notif~",
                        column: x => x.notification_dispatch_id,
                        principalTable: "notification_dispatch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_notification_notification_dispatch_id",
                table: "notification",
                column: "notification_dispatch_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_notification_template_id",
                table: "notification",
                column: "notification_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_user_id_read_at",
                table: "notification",
                columns: new[] { "user_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_dispatch_notification_template_id",
                table: "notification_dispatch",
                column: "notification_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_dispatch_recipient_notification_dispatch_id",
                table: "notification_dispatch_recipient",
                column: "notification_dispatch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "notification_dispatch_recipient");

            migrationBuilder.DropTable(
                name: "notification_dispatch");

            migrationBuilder.DropTable(
                name: "notification_template");
        }
    }
}
