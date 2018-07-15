using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class int_id_period : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitPeriods");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Periods",
                nullable: false);

            migrationBuilder.AddPrimaryKey("PK_Periods","Periods","Id");

            migrationBuilder.DropColumn("PeriodId","Commits");

            migrationBuilder.AddColumn<long>(
                name: "PeriodId",
                table: "Commits",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Periods",
                nullable: false,
                oldClrType: typeof(long))
                .OldAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "PeriodId",
                table: "Commits",
                nullable: true,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CommitPeriods",
                columns: table => new
                {
                    CommitSha = table.Column<string>(maxLength: 40, nullable: false),
                    PeriodId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitPeriods", x => new { x.CommitSha, x.PeriodId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitPeriods_CommitSha",
                table: "CommitPeriods",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_CommitPeriods_PeriodId",
                table: "CommitPeriods",
                column: "PeriodId");
        }
    }
}
