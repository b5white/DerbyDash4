using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DerbyDash.Migrations
{
    /// <inheritdoc />
    public partial class AddRacerLimitColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if AspNetUsers table exists before adding columns
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AspNetUsers]') AND name = 'IsRacerLimitOverridden')
                    BEGIN
                        ALTER TABLE [AspNetUsers] ADD [IsRacerLimitOverridden] bit NOT NULL DEFAULT CAST(0 AS bit);
                    END
                    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[AspNetUsers]') AND name = 'RacerLimit')
                    BEGIN
                        ALTER TABLE [AspNetUsers] ADD [RacerLimit] int NULL;
                    END
                END
            ");

            // Only create Feedbacks table if AspNetUsers exists
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[Feedbacks]', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [Feedbacks] (
                            [Id] int NOT NULL IDENTITY,
                            [UserId] nvarchar(450) NULL,
                            [RacerId] int NULL,
                            [Name] nvarchar(100) NOT NULL,
                            [Email] nvarchar(256) NOT NULL,
                            [FeedbackType] int NOT NULL,
                            [Subject] nvarchar(200) NOT NULL,
                            [Message] nvarchar(2000) NOT NULL,
                            [BrowserInfo] nvarchar(500) NOT NULL,
                            [ContactConsent] bit NOT NULL,
                            [SubmittedAt] datetime2 NOT NULL,
                            [IsResolved] bit NOT NULL,
                            [AdminNotes] nvarchar(1000) NULL,
                            [PublicResponse] nvarchar(2000) NULL,
                            [PrivateResponse] nvarchar(2000) NULL,
                            [ResolvedAt] datetime2 NULL,
                            CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_Feedbacks_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
                        );
                        CREATE INDEX [IX_Feedbacks_RacerId] ON [Feedbacks] ([RacerId]);
                        CREATE INDEX [IX_Feedbacks_UserId] ON [Feedbacks] ([UserId]);
                    END
                END
            ");

            // Create Log table if it doesn't exist
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Log]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Log] (
                        [Id] int NOT NULL IDENTITY,
                        [LogLevel] int NOT NULL,
                        [ThreadId] int NULL,
                        [EventId] int NULL,
                        [EventName] nvarchar(100) NULL,
                        [Message] nvarchar(500) NULL,
                        [UserId] nvarchar(450) NULL,
                        [RacerId] int NULL,
                        [SessionId] nvarchar(100) NULL,
                        [Category] nvarchar(100) NULL,
                        [ExceptionMessage] nvarchar(1000) NULL,
                        [ExceptionStackTrace] nvarchar(max) NULL,
                        [ExceptionSource] nvarchar(100) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        CONSTRAINT [PK_Log] PRIMARY KEY ([Id])
                    );
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropColumn(
                name: "IsRacerLimitOverridden",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RacerLimit",
                table: "AspNetUsers");
        }
    }
}
