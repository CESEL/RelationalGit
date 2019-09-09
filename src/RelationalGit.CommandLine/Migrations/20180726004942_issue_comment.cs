using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class issue_comment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    UserLogin = table.Column<string>(nullable: true),
                    IssueNumber = table.Column<long>(nullable: false),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    HtmltUrl = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueComments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_IssueNumber",
                table: "IssueComments",
                column: "IssueNumber");

            migrationBuilder.CreateIndex(
                name: "IX_IssueComments_UserLogin",
                table: "IssueComments",
                column: "UserLogin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueComments");
        }
    }
}
