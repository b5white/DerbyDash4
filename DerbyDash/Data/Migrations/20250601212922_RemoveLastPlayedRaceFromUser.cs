using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerbyDash.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLastPlayedRaceFromUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_AspNetUsers_UserId",
                table: "FamilyMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_Races_FamilyMembers_FamilyMemberId",
                table: "Races");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Races_FamilyMemberId",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_Id_FamilyMemberId_ProblemSetId",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_UserId",
                table: "FamilyMembers");

            migrationBuilder.DropColumn(
                name: "AvatarFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastPlayedRace",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarFileName",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPlayedRace",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RacerId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContactConsent = table.Column<bool>(type: "bit", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FeedbackType = table.Column<int>(type: "int", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Feedbacks_FamilyMembers_RacerId",
                        column: x => x.RacerId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Races_FamilyMemberId",
                table: "Races",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Id_FamilyMemberId_ProblemSetId",
                table: "Races",
                columns: new[] { "Id", "FamilyMemberId", "ProblemSetId" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_UserId",
                table: "FamilyMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_RacerId",
                table: "Feedbacks",
                column: "RacerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_AspNetUsers_UserId",
                table: "FamilyMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Races_FamilyMembers_FamilyMemberId",
                table: "Races",
                column: "FamilyMemberId",
                principalTable: "FamilyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
