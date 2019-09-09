using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class FileTouch_Improvements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commits",
                table: "FileTouches",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PullRequests",
                table: "FileTouches",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commits",
                table: "FileTouches");

            migrationBuilder.DropColumn(
                name: "PullRequests",
                table: "FileTouches");
        }
    }
}
