using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class filetouches_improve : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commits",
                table: "FileTouches");

            migrationBuilder.DropColumn(
                name: "PullRequests",
                table: "FileTouches");

            migrationBuilder.RenameColumn(
                name: "Reviewers",
                table: "FileKnowledgeables",
                newName: "AvailableReviewers");

            migrationBuilder.RenameColumn(
                name: "Committers",
                table: "FileKnowledgeables",
                newName: "AvailableCommitters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AvailableReviewers",
                table: "FileKnowledgeables",
                newName: "Reviewers");

            migrationBuilder.RenameColumn(
                name: "AvailableCommitters",
                table: "FileKnowledgeables",
                newName: "Committers");

            migrationBuilder.AddColumn<string>(
                name: "Commits",
                table: "FileTouches",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PullRequests",
                table: "FileTouches",
                nullable: true);
        }
    }
}
