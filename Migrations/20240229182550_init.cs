using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quizwebapp.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quiz_cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Thumbnail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Thumbnail = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Thumbnail = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshTokenToken = table.Column<string>(name: "RefreshToken_Token", type: "text", nullable: true),
                    RefreshTokenCreated = table.Column<DateTime>(name: "RefreshToken_Created", type: "timestamp with time zone", nullable: true),
                    RefreshTokenExpires = table.Column<DateTime>(name: "RefreshToken_Expires", type: "timestamp with time zone", nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    Playing = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_question_relation",
                columns: table => new
                {
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<bool>(type: "boolean", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_question_relation", x => new { x.QuestionId, x.CardId });
                    table.ForeignKey(
                        name: "FK_quiz_question_relation_quiz_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "quiz_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quiz_question_relation_quiz_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "quiz_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImageUser",
                columns: table => new
                {
                    AccessUsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImagesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageUser", x => new { x.AccessUsersId, x.ImagesId });
                    table.ForeignKey(
                        name: "FK_ImageUser_Images_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageUser_users_AccessUsersId",
                        column: x => x.AccessUsersId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Paid = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pays_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Award = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Thumbnail = table.Column<string>(type: "text", nullable: false),
                    QuestionsAmount = table.Column<int>(type: "integer", nullable: false),
                    Completed = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quizes_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedQuizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Elapsed = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Fulfilled = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Raiting = table.Column<int>(type: "integer", nullable: true),
                    CompetitiveType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedQuizes", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "QuizQuizCard",
                columns: table => new
                {
                    QuizCardsId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuizCard", x => new { x.QuizCardsId, x.QuizesId });
                    table.ForeignKey(
                        name: "FK_QuizQuizCard_quiz_cards_QuizCardsId",
                        column: x => x.QuizCardsId,
                        principalTable: "quiz_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizQuizCard_quizes_QuizesId",
                        column: x => x.QuizesId,
                        principalTable: "quizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardAnswer",
                columns: table => new
                {
                    CompletedId = table.Column<Guid>(type: "uuid", nullable: false),
                    CardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<bool>(type: "boolean", nullable: false),
                    Elapsed = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardAnswer", x => new { x.CompletedId, x.CardId });
                    table.ForeignKey(
                        name: "FK_CardAnswer_CompletedQuizes_CompletedId",
                        column: x => x.CompletedId,
                        principalTable: "CompletedQuizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardAnswer_quiz_cards_CardId",
                        column: x => x.CardId,
                        principalTable: "quiz_cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardAnswer_CardId",
                table: "CardAnswer",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuizes_QuizId",
                table: "CompletedQuizes",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedQuizes_UserId",
                table: "CompletedQuizes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageUser_ImagesId",
                table: "ImageUser",
                column: "ImagesId");

            migrationBuilder.CreateIndex(
                name: "IX_pays_UserId",
                table: "pays",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_question_relation_CardId",
                table: "quiz_question_relation",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_quizes_CreatorId",
                table: "quizes",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuizCard_QuizesId",
                table: "QuizQuizCard",
                column: "QuizesId");

            migrationBuilder.CreateIndex(
                name: "IX_users_GroupId",
                table: "users",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardAnswer");

            migrationBuilder.DropTable(
                name: "ImageUser");

            migrationBuilder.DropTable(
                name: "pays");

            migrationBuilder.DropTable(
                name: "quiz_question_relation");

            migrationBuilder.DropTable(
                name: "QuizQuizCard");

            migrationBuilder.DropTable(
                name: "CompletedQuizes");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "quiz_cards");

            migrationBuilder.DropTable(
                name: "quizes");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
