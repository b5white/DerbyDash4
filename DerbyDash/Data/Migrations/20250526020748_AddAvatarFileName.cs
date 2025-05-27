using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerbyDash.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarFileName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActiveFamilyMemberId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastPlayedRace",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastPlayedTime",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TeamRaceCount",
                table: "AspNetUsers");

            // Add temporary column for FeedbackType conversion
            migrationBuilder.AddColumn<int>(
                name: "FeedbackType_New",
                table: "Feedbacks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Convert existing string values to enum values
            migrationBuilder.Sql(@"
                UPDATE Feedbacks 
                SET FeedbackType_New = CASE 
                    WHEN FeedbackType = 'Bug Report' OR FeedbackType = 'BugReport' THEN 1
                    WHEN FeedbackType = 'Feature Request' OR FeedbackType = 'FeatureRequest' THEN 2
                    WHEN FeedbackType = 'General Feedback' OR FeedbackType = 'GeneralFeedback' THEN 3
                    WHEN FeedbackType = 'Question' THEN 4
                    ELSE 0
                END");

            // Drop the old FeedbackType column
            migrationBuilder.DropColumn(
                name: "FeedbackType",
                table: "Feedbacks");

            // Rename the new column to FeedbackType
            migrationBuilder.RenameColumn(
                name: "FeedbackType_New",
                table: "Feedbacks",
                newName: "FeedbackType");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Feedbacks",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "Feedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "Feedbacks",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarFileName",
                table: "AspNetUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert FeedbackType back to string
            migrationBuilder.AddColumn<string>(
                name: "FeedbackType_Old",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Convert enum values back to strings
            migrationBuilder.Sql(@"
                UPDATE Feedbacks 
                SET FeedbackType_Old = CASE 
                    WHEN FeedbackType = 1 THEN 'Bug Report'
                    WHEN FeedbackType = 2 THEN 'Feature Request'
                    WHEN FeedbackType = 3 THEN 'General Feedback'
                    WHEN FeedbackType = 4 THEN 'Question'
                    ELSE 'General Feedback'
                END");

            // Drop the enum column
            migrationBuilder.DropColumn(
                name: "FeedbackType",
                table: "Feedbacks");

            // Rename back to FeedbackType
            migrationBuilder.RenameColumn(
                name: "FeedbackType_Old",
                table: "Feedbacks",
                newName: "FeedbackType");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "BrowserInfo",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "AdminNotes",
                table: "Feedbacks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AvatarFileName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ActiveFamilyMemberId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPlayedRace",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPlayedTime",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamRaceCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
