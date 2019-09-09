using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class add_participation_date : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FirstCommitDateTime",
                table: "Developers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstReviewDateTime",
                table: "Developers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCommitDateTime",
                table: "Developers",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReviewDateTime",
                table: "Developers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstCommitDateTime",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "FirstReviewDateTime",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "LastCommitDateTime",
                table: "Developers");

            migrationBuilder.DropColumn(
                name: "LastReviewDateTime",
                table: "Developers");
        }
    }
}
