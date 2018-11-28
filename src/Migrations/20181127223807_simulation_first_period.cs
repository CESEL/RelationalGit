using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class simulation_first_period : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirstPeriod",
                table: "LossSimulations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstPeriod",
                table: "LossSimulations");
        }
    }
}
