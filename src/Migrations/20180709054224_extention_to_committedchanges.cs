using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class extention_to_committedchanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "CommittedChanges",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "CommittedChanges",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTest",
                table: "CommittedChanges",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extension",
                table: "CommittedChanges");

            migrationBuilder.DropColumn(
                name: "FileType",
                table: "CommittedChanges");

            migrationBuilder.DropColumn(
                name: "IsTest",
                table: "CommittedChanges");
        }
    }
}
