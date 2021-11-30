using Microsoft.EntityFrameworkCore.Migrations;

namespace DataAccessLayer.Migrations
{
    public partial class ModifyColumnNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "dateOfBirth",
                table: "Users",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "darkTheme",
                table: "Users",
                newName: "DarkTheme");

            migrationBuilder.RenameColumn(
                name: "createdDate",
                table: "Users",
                newName: "CreatedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Users",
                newName: "dateOfBirth");

            migrationBuilder.RenameColumn(
                name: "DarkTheme",
                table: "Users",
                newName: "darkTheme");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Users",
                newName: "createdDate");
        }
    }
}
