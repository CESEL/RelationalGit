using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class RenameCommitedFileToCommittedChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "CommittedFiles",newName: "CommittedChanges");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
