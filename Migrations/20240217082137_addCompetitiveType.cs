using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class addCompetitiveType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompetitiveType",
                table: "CompletedQuizes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetitiveType",
                table: "CompletedQuizes");
        }
    }
}
