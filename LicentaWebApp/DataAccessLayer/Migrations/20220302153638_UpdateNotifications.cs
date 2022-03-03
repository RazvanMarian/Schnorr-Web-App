using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class UpdateNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationsUserStatus_Users_NotifiedUserId",
                table: "NotificationsUserStatus");

            migrationBuilder.DropIndex(
                name: "IX_NotificationsUserStatus_NotifiedUserId",
                table: "NotificationsUserStatus");

            migrationBuilder.AlterColumn<int>(
                name: "NotifiedUserId",
                table: "NotificationsUserStatus",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedKey",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedKey",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "NotifiedUserId",
                table: "NotificationsUserStatus",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsUserStatus_NotifiedUserId",
                table: "NotificationsUserStatus",
                column: "NotifiedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationsUserStatus_Users_NotifiedUserId",
                table: "NotificationsUserStatus",
                column: "NotifiedUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
