using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class add_authorname_commit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Commits",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommitterName",
                table: "Commits",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "Commits");

            migrationBuilder.DropColumn(
                name: "CommitterName",
                table: "Commits");
        }
    }
}
