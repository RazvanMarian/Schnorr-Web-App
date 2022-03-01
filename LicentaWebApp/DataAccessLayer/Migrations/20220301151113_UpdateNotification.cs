using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    public partial class UpdateNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationId",
                table: "NotificationsUserStatus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedKeyName",
                table: "NotificationsUserStatus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationsUserStatus_NotificationId",
                table: "NotificationsUserStatus",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationsUserStatus_Notifications_NotificationId",
                table: "NotificationsUserStatus",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NotificationsUserStatus_Notifications_NotificationId",
                table: "NotificationsUserStatus");

            migrationBuilder.DropIndex(
                name: "IX_NotificationsUserStatus_NotificationId",
                table: "NotificationsUserStatus");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "NotificationsUserStatus");

            migrationBuilder.DropColumn(
                name: "SelectedKeyName",
                table: "NotificationsUserStatus");
        }
    }
}
