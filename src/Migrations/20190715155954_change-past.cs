using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class changepast : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ChangePast",
                table: "LossSimulations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangePast",
                table: "LossSimulations");
        }
    }
}
