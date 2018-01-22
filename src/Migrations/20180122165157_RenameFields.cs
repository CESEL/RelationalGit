using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class RenameFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PullRequestReviewers_CommitId",
                table: "PullRequestReviewers");

            migrationBuilder.DropIndex(
                name: "IX_PullRequestReviewers_PullRequestNumber",
                table: "PullRequestReviewers");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "PullRequestReviewerComments",
                newName: "UserLogin");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PullRequestReviewerComments",
                newName: "CreatedAtDateTime");

            migrationBuilder.RenameIndex(
                name: "IX_PullRequestReviewerComments_Username",
                table: "PullRequestReviewerComments",
                newName: "IX_PullRequestReviewerComments_UserLogin");

            migrationBuilder.RenameColumn(
                name: "Oid",
                table: "PullRequestFiles",
                newName: "Sha");

            migrationBuilder.AlterColumn<string>(
                name: "CommitId",
                table: "PullRequestReviewers",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserLogin",
                table: "PullRequestReviewerComments",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "CreatedAtDateTime",
                table: "PullRequestReviewerComments",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_PullRequestReviewerComments_UserLogin",
                table: "PullRequestReviewerComments",
                newName: "IX_PullRequestReviewerComments_Username");

            migrationBuilder.RenameColumn(
                name: "Sha",
                table: "PullRequestFiles",
                newName: "Oid");

            migrationBuilder.AlterColumn<string>(
                name: "CommitId",
                table: "PullRequestReviewers",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewers_CommitId",
                table: "PullRequestReviewers",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestReviewers_PullRequestNumber",
                table: "PullRequestReviewers",
                column: "PullRequestNumber");
        }
    }
}
