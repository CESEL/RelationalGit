using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class index_normalized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NormalizedAuthorName",
                table: "Commits",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorName",
                table: "Commits",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedDeveloperIdentity",
                table: "CommitBlobBlames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Commits_AuthorName",
                table: "Commits",
                column: "AuthorName");

            migrationBuilder.CreateIndex(
                name: "IX_Commits_NormalizedAuthorName",
                table: "Commits",
                column: "NormalizedAuthorName");

            migrationBuilder.CreateIndex(
                name: "IX_CommitBlobBlames_NormalizedDeveloperIdentity",
                table: "CommitBlobBlames",
                column: "NormalizedDeveloperIdentity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Commits_AuthorName",
                table: "Commits");

            migrationBuilder.DropIndex(
                name: "IX_Commits_NormalizedAuthorName",
                table: "Commits");

            migrationBuilder.DropIndex(
                name: "IX_CommitBlobBlames_NormalizedDeveloperIdentity",
                table: "CommitBlobBlames");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedAuthorName",
                table: "Commits",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorName",
                table: "Commits",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedDeveloperIdentity",
                table: "CommitBlobBlames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
