using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharjahConferenceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationAndAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:announcement_priority", "low,normal,high,urgent")
                .Annotation("Npgsql:Enum:meeting_status", "scheduled,in_progress,completed,cancelled,no_show")
                .Annotation("Npgsql:Enum:table_status", "available,reserved,occupied")
                .Annotation("Npgsql:Enum:user_role", "admin,publisher,participant")
                .OldAnnotation("Npgsql:Enum:announcement_priority", "low,normal,high,urgent")
                .OldAnnotation("Npgsql:Enum:meeting_status", "scheduled,in_progress,completed,cancelled,no_show")
                .OldAnnotation("Npgsql:Enum:table_status", "available,reserved,occupied");

            migrationBuilder.AddColumn<Guid>(
                name: "ReservedByPublisherId",
                table: "MeetingTables",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    PublisherId = table.Column<Guid>(type: "uuid", nullable: true),
                    ParticipantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Users_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTables_ReservedByPublisherId",
                table: "MeetingTables",
                column: "ReservedByPublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ParticipantId",
                table: "Users",
                column: "ParticipantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PublisherId",
                table: "Users",
                column: "PublisherId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTables_Publishers_ReservedByPublisherId",
                table: "MeetingTables",
                column: "ReservedByPublisherId",
                principalTable: "Publishers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTables_Publishers_ReservedByPublisherId",
                table: "MeetingTables");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_MeetingTables_ReservedByPublisherId",
                table: "MeetingTables");

            migrationBuilder.DropColumn(
                name: "ReservedByPublisherId",
                table: "MeetingTables");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:announcement_priority", "low,normal,high,urgent")
                .Annotation("Npgsql:Enum:meeting_status", "scheduled,in_progress,completed,cancelled,no_show")
                .Annotation("Npgsql:Enum:table_status", "available,reserved,occupied")
                .OldAnnotation("Npgsql:Enum:announcement_priority", "low,normal,high,urgent")
                .OldAnnotation("Npgsql:Enum:meeting_status", "scheduled,in_progress,completed,cancelled,no_show")
                .OldAnnotation("Npgsql:Enum:table_status", "available,reserved,occupied")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,publisher,participant");
        }
    }
}
