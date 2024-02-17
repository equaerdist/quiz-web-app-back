using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class io : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "Playing",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Playing = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_GroupId",
                table: "users",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_Groups_GroupId",
                table: "users",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_Groups_GroupId",
                table: "users");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_users_GroupId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Playing",
                table: "users");
        }
    }
}
