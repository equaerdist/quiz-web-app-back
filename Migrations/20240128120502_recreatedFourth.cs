using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class recreatedFourth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_quizes_QuizId",
                table: "CardAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_users_UserId",
                table: "CardAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardAnswer",
                table: "CardAnswer");

            migrationBuilder.DropIndex(
                name: "IX_CardAnswer_QuizId",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "CardAnswer");

            migrationBuilder.RenameColumn(
                name: "QuizId",
                table: "CardAnswer",
                newName: "CompletedId");

            migrationBuilder.AlterColumn<bool>(
                name: "Type",
                table: "CardAnswer",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedEndTime",
                table: "CardAnswer",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedQuizId",
                table: "CardAnswer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletedUserId",
                table: "CardAnswer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Elapsed",
                table: "CardAnswer",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "CardAnswer",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardAnswer",
                table: "CardAnswer",
                columns: new[] { "CompletedId", "CardId" });

            migrationBuilder.CreateTable(
                name: "CompletedQuizes",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Elapsed = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Fulfilled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedQuizes", x => new { x.QuizId, x.UserId, x.EndTime });
                    table.ForeignKey(
                        name: "FK_CompletedQuizes_quizes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "quizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompletedQuizes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedEndTime",
                table: "CardAnswer",
                columns: new[] { "CompletedQuizId", "CompletedUserId", "CompletedEndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuizes_UserId",
                table: "CompletedQuizes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardAnswer_CompletedQuizes_CompletedQuizId_CompletedUserId_~",
                table: "CardAnswer",
                columns: new[] { "CompletedQuizId", "CompletedUserId", "CompletedEndTime" },
                principalTable: "CompletedQuizes",
                principalColumns: new[] { "QuizId", "UserId", "EndTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_CompletedQuizes_CompletedQuizId_CompletedUserId_~",
                table: "CardAnswer");

            migrationBuilder.DropTable(
                name: "CompletedQuizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardAnswer",
                table: "CardAnswer");

            migrationBuilder.DropIndex(
                name: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedEndTime",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "CompletedEndTime",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "CompletedQuizId",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "CompletedUserId",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "Elapsed",
                table: "CardAnswer");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "CardAnswer");

            migrationBuilder.RenameColumn(
                name: "CompletedId",
                table: "CardAnswer",
                newName: "QuizId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CardAnswer",
                type: "text",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "CardAnswer",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardAnswer",
                table: "CardAnswer",
                columns: new[] { "UserId", "QuizId", "CardId" });

            migrationBuilder.CreateIndex(
                name: "IX_CardAnswer_QuizId",
                table: "CardAnswer",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardAnswer_quizes_QuizId",
                table: "CardAnswer",
                column: "QuizId",
                principalTable: "quizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardAnswer_users_UserId",
                table: "CardAnswer",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
