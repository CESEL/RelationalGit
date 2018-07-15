using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class ignore_index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Commits_Ignore",
                table: "Commits",
                column: "Ignore");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlames_Ignore",
                table: "CommitBlobBlames",
                column: "Ignore");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Commits_Ignore",
                table: "Commits");

            migrationBuilder.DropIndex(
                name: "IX_CommitBlobBlames_Ignore",
                table: "CommitBlobBlames");
        }
    }
}
