using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IP.Project.Migrations
{
    /// <inheritdoc />
    public partial class AddedEsimsDataToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CNP",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cn",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "displayName",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "employeeNumber",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "gidNumber",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "givenName",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "homeDirectory",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "homePhone",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "initials",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "localityName",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "mail",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "mobile",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ou",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "postalCode",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "roomNumber",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "shadowInactive",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "sn",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "telephoneNumber",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "uid",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "uidNumber",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "userPassword",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CNP",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "cn",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "displayName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "employeeNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "gidNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "givenName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "homeDirectory",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "homePhone",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "initials",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "localityName",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "mail",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "mobile",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ou",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "postalCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "roomNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "shadowInactive",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "sn",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "street",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "telephoneNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "title",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "uid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "uidNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "userPassword",
                table: "Accounts");
        }
    }
}
