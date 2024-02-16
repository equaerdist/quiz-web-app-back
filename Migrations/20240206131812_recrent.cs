using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class recrent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Raiting",
                table: "CompletedQuizes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Raiting",
                table: "CompletedQuizes");
        }
    }
}
