using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Data.Migrtions
{
    /// <inheritdoc />
    public partial class AddClassDetailsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassDetail_ClassRooms_ClassRoomId",
                table: "ClassDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassDetail_Users_UserId",
                table: "ClassDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassDetail",
                table: "ClassDetail");

            migrationBuilder.RenameTable(
                name: "ClassDetail",
                newName: "ClassDetails");

            migrationBuilder.RenameIndex(
                name: "IX_ClassDetail_UserId",
                table: "ClassDetails",
                newName: "IX_ClassDetails_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassDetails",
                table: "ClassDetails",
                columns: new[] { "ClassRoomId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassDetails_ClassRooms_ClassRoomId",
                table: "ClassDetails",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassDetails_Users_UserId",
                table: "ClassDetails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassDetails_ClassRooms_ClassRoomId",
                table: "ClassDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassDetails_Users_UserId",
                table: "ClassDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassDetails",
                table: "ClassDetails");

            migrationBuilder.RenameTable(
                name: "ClassDetails",
                newName: "ClassDetail");

            migrationBuilder.RenameIndex(
                name: "IX_ClassDetails_UserId",
                table: "ClassDetail",
                newName: "IX_ClassDetail_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassDetail",
                table: "ClassDetail",
                columns: new[] { "ClassRoomId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClassDetail_ClassRooms_ClassRoomId",
                table: "ClassDetail",
                column: "ClassRoomId",
                principalTable: "ClassRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassDetail_Users_UserId",
                table: "ClassDetail",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
