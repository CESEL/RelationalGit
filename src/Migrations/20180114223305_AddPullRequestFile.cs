using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class AddPullRequestFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PullRequestFiles",
                columns: table => new
                {
                    Oid = table.Column<string>(nullable: false),
                    Additions = table.Column<int>(nullable: true),
                    Changes = table.Column<int>(nullable: true),
                    Deletions = table.Column<int>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    PullRequestNumber = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestFiles", x => x.Oid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestFiles_FileName",
                table: "PullRequestFiles",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestFiles_PullRequestNumber",
                table: "PullRequestFiles",
                column: "PullRequestNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PullRequestFiles");
        }
    }
}
