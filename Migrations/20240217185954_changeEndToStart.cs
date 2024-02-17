using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class changeEndToStart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "CompletedQuizes",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "CompletedEndTime",
                table: "CardAnswer",
                newName: "CompletedStartTime");

            migrationBuilder.RenameIndex(
                name: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedEndTime",
                table: "CardAnswer",
                newName: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedStartTi~");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "CompletedQuizes",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "CompletedStartTime",
                table: "CardAnswer",
                newName: "CompletedEndTime");

            migrationBuilder.RenameIndex(
                name: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedStartTi~",
                table: "CardAnswer",
                newName: "IX_CardAnswer_CompletedQuizId_CompletedUserId_CompletedEndTime");
        }
    }
}
