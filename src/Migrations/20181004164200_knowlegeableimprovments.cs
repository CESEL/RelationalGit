using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class knowlegeableimprovments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Committers",
                table: "FileKnowledgeables",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reviewers",
                table: "FileKnowledgeables",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalCommitOnly",
                table: "FileKnowledgeables",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCommitters",
                table: "FileKnowledgeables",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviewOnly",
                table: "FileKnowledgeables",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviewers",
                table: "FileKnowledgeables",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Committers",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "Reviewers",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "TotalCommitOnly",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "TotalCommitters",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "TotalReviewOnly",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "TotalReviewers",
                table: "FileKnowledgeables");
        }
    }
}
