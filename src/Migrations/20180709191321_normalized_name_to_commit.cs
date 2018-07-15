using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class normalized_name_to_commit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedAuthorName",
                table: "Commits",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedDeveloperIdentity",
                table: "CommitBlobBlames",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NormalizedAuthorName",
                table: "Commits");

            migrationBuilder.DropColumn(
                name: "NormalizedDeveloperIdentity",
                table: "CommitBlobBlames");
        }
    }
}
