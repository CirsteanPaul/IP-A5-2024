using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IP.Project.Migrations
{
    /// <inheritdoc />
    public partial class AddedEmailVariantsToAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mailVariant1",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "mailVariant2",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "mailVariant3",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mailVariant1",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "mailVariant2",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "mailVariant3",
                table: "Accounts");
        }
    }
}
