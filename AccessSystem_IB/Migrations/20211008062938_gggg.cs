using Microsoft.EntityFrameworkCore.Migrations;

namespace AccessSystem_IB.Migrations
{
    public partial class gggg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAuthInfos_Users_UserLogin",
                table: "UserAuthInfos");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAuthInfos_Users_UserLogin",
                table: "UserAuthInfos",
                column: "UserLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAuthInfos_Users_UserLogin",
                table: "UserAuthInfos");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAuthInfos_Users_UserLogin",
                table: "UserAuthInfos",
                column: "UserLogin",
                principalTable: "Users",
                principalColumn: "Login",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
