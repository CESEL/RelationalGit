using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class index_aliased_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "AliasedDeveloperNames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AliasedDeveloperNames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AliasedDeveloperNames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AliasedDeveloperNames_Email",
                table: "AliasedDeveloperNames",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AliasedDeveloperNames_Name",
                table: "AliasedDeveloperNames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AliasedDeveloperNames_NormalizedName",
                table: "AliasedDeveloperNames",
                column: "NormalizedName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AliasedDeveloperNames_Email",
                table: "AliasedDeveloperNames");

            migrationBuilder.DropIndex(
                name: "IX_AliasedDeveloperNames_Name",
                table: "AliasedDeveloperNames");

            migrationBuilder.DropIndex(
                name: "IX_AliasedDeveloperNames_NormalizedName",
                table: "AliasedDeveloperNames");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedName",
                table: "AliasedDeveloperNames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AliasedDeveloperNames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "AliasedDeveloperNames",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
