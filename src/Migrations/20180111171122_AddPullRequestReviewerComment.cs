using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class AddPullRequestReviewerComment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MergeCommitSha",
                table: "PullRequests",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BaseCommitSha",
                table: "PullRequests",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommitId",
                table: "PullRequestReviewers",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PullRequestReviewerComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    CommitId = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    InReplyTo = table.Column<int>(nullable: true),
                    Path = table.Column<string>(nullable: true),
                    PullRequestReviewId = table.Column<int>(nullable: true),
                    PullRequestUrl = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    Username = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestReviewerComments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_BaseCommitSha",
                table: "PullRequests",
                column: "BaseCommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_MergeCommitSha",
                table: "PullRequests",
                column: "MergeCommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_Number",
                table: "PullRequests",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewers_CommitId",
                table: "PullRequestReviewers",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewers_PullRequestNumber",
                table: "PullRequestReviewers",
                column: "PullRequestNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_CommitId",
                table: "PullRequestReviewerComments",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_Path",
                table: "PullRequestReviewerComments",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_PullRequestReviewId",
                table: "PullRequestReviewerComments",
                column: "PullRequestReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewerComments_Username",
                table: "PullRequestReviewerComments",
                column: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PullRequestReviewerComments");

            migrationBuilder.DropIndex(
                name: "IX_PullRequests_BaseCommitSha",
                table: "PullRequests");

            migrationBuilder.DropIndex(
                name: "IX_PullRequests_MergeCommitSha",
                table: "PullRequests");

            migrationBuilder.DropIndex(
                name: "IX_PullRequests_Number",
                table: "PullRequests");

            migrationBuilder.DropIndex(
                name: "IX_PullRequestReviewers_CommitId",
                table: "PullRequestReviewers");

            migrationBuilder.DropIndex(
                name: "IX_PullRequestReviewers_PullRequestNumber",
                table: "PullRequestReviewers");

            migrationBuilder.AlterColumn<string>(
                name: "MergeCommitSha",
                table: "PullRequests",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BaseCommitSha",
                table: "PullRequests",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommitId",
                table: "PullRequestReviewers",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
