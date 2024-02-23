using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class @int : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_CompletedQuizes_CompletedQuizId_CompletedUserId_~",
                table: "CardAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletedQuizes",
                table: "CompletedQuizes");

            migrationBuilder.DropIndex(
                name: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedStartTi~",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "CompletedQuizId",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "CompletedStartTime",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "CompletedUserId",
                table: "CardAnswer");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "CompletedQuizes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletedQuizes",
                table: "CompletedQuizes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuizes_QuizId",
                table: "CompletedQuizes",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardAnswer_CompletedQuizes_CompletedId",
                table: "CardAnswer",
                column: "CompletedId",
                principalTable: "CompletedQuizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_CompletedQuizes_CompletedId",
                table: "CardAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompletedQuizes",
                table: "CompletedQuizes");

            migrationBuilder.DropIndex(
                name: "IX_CompletedQuizes_QuizId",
                table: "CompletedQuizes");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CompletedQuizes");

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedQuizId",
                table: "CardAnswer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedStartTime",
                table: "CardAnswer",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedUserId",
                table: "CardAnswer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompletedQuizes",
                table: "CompletedQuizes",
                columns: new[] { "QuizId", "UserId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedStartTi~",
                table: "CardAnswer",
                columns: new[] { "CompletedQuizId", "CompletedUserId", "CompletedStartTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_CardAnswer_CompletedQuizes_CompletedQuizId_CompletedUserId_~",
                table: "CardAnswer",
                columns: new[] { "CompletedQuizId", "CompletedUserId", "CompletedStartTime" },
                principalTable: "CompletedQuizes",
                principalColumns: new[] { "QuizId", "UserId", "StartTime" });
        }
    }
}
