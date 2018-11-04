using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class Total_Owner_Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalCommitters",
                table: "FileKnowledgeables",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalPullRequests",
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
                name: "TotalCommitters",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "TotalPullRequests",
                table: "FileKnowledgeables");

            migrationBuilder.DropColumn(
                name: "TotalReviewers",
                table: "FileKnowledgeables");
        }
    }
}
