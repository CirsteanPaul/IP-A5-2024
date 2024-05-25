using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IP.Project.Migrations
{
    /// <inheritdoc />
    public partial class AddedMailAlternativeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mailAlternateAddress",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mailAlternateAddress",
                table: "Accounts");
        }
    }
}
