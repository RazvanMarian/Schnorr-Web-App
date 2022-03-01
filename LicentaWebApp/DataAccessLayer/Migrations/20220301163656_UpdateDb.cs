using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class UpdateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_InitiatorId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "InitiatorId",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_InitiatorId",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.AddColumn<int>(
                name: "IdInitiator",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "IdInitiator",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "InitiatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_InitiatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_InitiatorId",
                table: "Notifications",
                column: "InitiatorId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
