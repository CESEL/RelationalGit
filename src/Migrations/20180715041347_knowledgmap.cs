using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class knowledgmap : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KnowledgeSaveStrategyType",
                table: "LossSimulations",
                newName: "KnowledgeShareStrategyType");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDateTime",
                table: "LossSimulations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "FileTouches",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NormalizeDeveloperName = table.Column<string>(nullable: true),
                    PeriodId = table.Column<long>(nullable: false),
                    CanonicalPath = table.Column<string>(nullable: true),
                    TouchType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTouches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecommendedPullRequestReviewers",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PullRequestNumber = table.Column<long>(nullable: false),
                    NormalizedReviewerName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendedPullRequestReviewers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_CanonicalPath",
                table: "FileTouches",
                column: "CanonicalPath");

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_NormalizeDeveloperName",
                table: "FileTouches",
                column: "NormalizeDeveloperName");

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_PeriodId",
                table: "FileTouches",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTouches_TouchType",
                table: "FileTouches",
                column: "TouchType");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestReviewers_NormalizedReviewerName",
                table: "RecommendedPullRequestReviewers",
                column: "NormalizedReviewerName");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedPullRequestReviewers_PullRequestNumber",
                table: "RecommendedPullRequestReviewers",
                column: "PullRequestNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileTouches");

            migrationBuilder.DropTable(
                name: "RecommendedPullRequestReviewers");

            migrationBuilder.DropColumn(
                name: "EndDateTime",
                table: "LossSimulations");

            migrationBuilder.RenameColumn(
                name: "KnowledgeShareStrategyType",
                table: "LossSimulations",
                newName: "KnowledgeSaveStrategyType");
        }
    }
}
