using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class ChangePullRequestModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "PullRequests",
                newName: "UserLogin");

            migrationBuilder.RenameColumn(
                name: "MergedAt",
                table: "PullRequests",
                newName: "MergedAtDateTime");

            migrationBuilder.RenameColumn(
                name: "IsMerged",
                table: "PullRequests",
                newName: "Merged");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PullRequests",
                newName: "CreatedAtDateTime");

            migrationBuilder.RenameColumn(
                name: "ClosedAt",
                table: "PullRequests",
                newName: "ClosedAtDateTime");

            migrationBuilder.RenameColumn(
                name: "BaseCommitSha",
                table: "PullRequests",
                newName: "BaseSha");

            migrationBuilder.RenameIndex(
                name: "IX_PullRequests_BaseCommitSha",
                table: "PullRequests",
                newName: "IX_PullRequests_BaseSha");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserLogin",
                table: "PullRequests",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "MergedAtDateTime",
                table: "PullRequests",
                newName: "MergedAt");

            migrationBuilder.RenameColumn(
                name: "Merged",
                table: "PullRequests",
                newName: "IsMerged");

            migrationBuilder.RenameColumn(
                name: "CreatedAtDateTime",
                table: "PullRequests",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ClosedAtDateTime",
                table: "PullRequests",
                newName: "ClosedAt");

            migrationBuilder.RenameColumn(
                name: "BaseSha",
                table: "PullRequests",
                newName: "BaseCommitSha");

            migrationBuilder.RenameIndex(
                name: "IX_PullRequests_BaseSha",
                table: "PullRequests",
                newName: "IX_PullRequests_BaseCommitSha");
        }
    }
}
