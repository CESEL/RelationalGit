using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class addIssueEvent1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IssueEvents",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    ActorLogin = table.Column<string>(nullable: true),
                    CommitId = table.Column<string>(nullable: true),
                    CreatedAtDateTime = table.Column<DateTime>(nullable: false),
                    Event = table.Column<string>(nullable: true),
                    IssueNumber = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueEvents_CommitId",
                table: "IssueEvents",
                column: "CommitId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueEvents_Event",
                table: "IssueEvents",
                column: "Event");

            migrationBuilder.CreateIndex(
                name: "IX_IssueEvents_IssueNumber",
                table: "IssueEvents",
                column: "IssueNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueEvents");
        }
    }
}
