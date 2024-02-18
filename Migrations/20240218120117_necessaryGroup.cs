using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class necessaryGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_Groups_GroupId",
                table: "users");

            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "users",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_users_Groups_GroupId",
                table: "users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_Groups_GroupId",
                table: "users");

            migrationBuilder.AlterColumn<Guid>(
                name: "GroupId",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_users_Groups_GroupId",
                table: "users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
