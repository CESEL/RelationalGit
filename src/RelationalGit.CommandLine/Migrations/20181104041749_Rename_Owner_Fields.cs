using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class Rename_Owner_Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalReviewers",
                table: "FileKnowledgeables",
                newName: "TotalAvailableReviewers");

            migrationBuilder.RenameColumn(
                name: "TotalReviewOnly",
                table: "FileKnowledgeables",
                newName: "TotalAvailableReviewOnly");

            migrationBuilder.RenameColumn(
                name: "TotalCommitters",
                table: "FileKnowledgeables",
                newName: "TotalAvailableCommitters");

            migrationBuilder.RenameColumn(
                name: "TotalCommitOnly",
                table: "FileKnowledgeables",
                newName: "TotalAvailableCommitOnly");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAvailableReviewers",
                table: "FileKnowledgeables",
                newName: "TotalReviewers");

            migrationBuilder.RenameColumn(
                name: "TotalAvailableReviewOnly",
                table: "FileKnowledgeables",
                newName: "TotalReviewOnly");

            migrationBuilder.RenameColumn(
                name: "TotalAvailableCommitters",
                table: "FileKnowledgeables",
                newName: "TotalCommitters");

            migrationBuilder.RenameColumn(
                name: "TotalAvailableCommitOnly",
                table: "FileKnowledgeables",
                newName: "TotalCommitOnly");
        }
    }
}
