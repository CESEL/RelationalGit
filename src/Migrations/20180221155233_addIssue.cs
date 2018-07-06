using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class addIssue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Issue",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    Body = table.Column<string>(nullable: true),
                    ClosedAtDateTime = table.Column<DateTime>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: true),
                    HtmlUrl = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Number = table.Column<int>(nullable: false),
                    PullRequestNumber = table.Column<string>(nullable: true),
                    PullRequestUrl = table.Column<string>(nullable: true),
                    State = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    UserLogin = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issue", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issue_Number",
                table: "Issue",
                column: "Number");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Issue");
        }
    }
}
