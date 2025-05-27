using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerbyDash.Migrations
{
    /// <inheritdoc />
    public partial class CleanupInvalidRacerData : Migration
    {        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, clean up any invalid UserId references in FamilyMembers table
            migrationBuilder.Sql(@"
                DELETE FROM FamilyMembers 
                WHERE UserId NOT IN (SELECT Id FROM AspNetUsers)
                   OR UserId IS NULL 
                   OR UserId = ''
            ");

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamilyMemberId = table.Column<int>(type: "int", nullable: false),
                    RaceDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTime = table.Column<double>(type: "float", nullable: false),
                    ProblemSetId = table.Column<int>(type: "int", nullable: false),
                    ImageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Races_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeedIncrements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RaceId = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<double>(type: "float", nullable: false),
                    Speed = table.Column<double>(type: "float", nullable: false),
                    Distance = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeedIncrements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeedIncrements_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_UserId",
                table: "FamilyMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_FamilyMemberId",
                table: "Races",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_Id_FamilyMemberId_ProblemSetId",
                table: "Races",
                columns: new[] { "Id", "FamilyMemberId", "ProblemSetId" });

            migrationBuilder.CreateIndex(
                name: "IX_SpeedIncrements_RaceId",
                table: "SpeedIncrements",
                column: "RaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_FamilyMembers_AspNetUsers_UserId",
                table: "FamilyMembers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FamilyMembers_AspNetUsers_UserId",
                table: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "SpeedIncrements");

            migrationBuilder.DropTable(
                name: "Races");

            migrationBuilder.DropIndex(
                name: "IX_FamilyMembers_UserId",
                table: "FamilyMembers");
        }
    }
}
