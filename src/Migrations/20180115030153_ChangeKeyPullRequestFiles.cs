using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RelationalGit.Migrations
{
    public partial class ChangeKeyPullRequestFiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PullRequestFiles",
                table: "PullRequestFiles");

            migrationBuilder.AlterColumn<string>(
                name: "Oid",
                table: "PullRequestFiles",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "PullRequestFiles",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PullRequestFiles",
                table: "PullRequestFiles",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PullRequestFiles",
                table: "PullRequestFiles");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PullRequestFiles");

            migrationBuilder.AlterColumn<string>(
                name: "Oid",
                table: "PullRequestFiles",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PullRequestFiles",
                table: "PullRequestFiles",
                column: "Oid");
        }
    }
}
