using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RelationalGit.Migrations
{
    public partial class identity_key_blame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CommittedBlob",
                table: "CommittedBlob");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommitBlobBlames",
                table: "CommitBlobBlames");

            migrationBuilder.AlterColumn<string>(
                name: "CanonicalPath",
                table: "CommittedBlob",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "CommitSha",
                table: "CommittedBlob",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "CommittedBlob",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "CanonicalPath",
                table: "CommitBlobBlames",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "CommitSha",
                table: "CommitBlobBlames",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "DeveloperIdentity",
                table: "CommitBlobBlames",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "CommitBlobBlames",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommittedBlob",
                table: "CommittedBlob",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommitBlobBlames",
                table: "CommitBlobBlames",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CommittedBlob",
                table: "CommittedBlob");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommitBlobBlames",
                table: "CommitBlobBlames");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CommittedBlob");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CommitBlobBlames");

            migrationBuilder.AlterColumn<string>(
                name: "CommitSha",
                table: "CommittedBlob",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CanonicalPath",
                table: "CommittedBlob",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeveloperIdentity",
                table: "CommitBlobBlames",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CommitSha",
                table: "CommitBlobBlames",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CanonicalPath",
                table: "CommitBlobBlames",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommittedBlob",
                table: "CommittedBlob",
                columns: new[] { "CommitSha", "CanonicalPath" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommitBlobBlames",
                table: "CommitBlobBlames",
                columns: new[] { "DeveloperIdentity", "CommitSha", "CanonicalPath" });
        }
    }
}
