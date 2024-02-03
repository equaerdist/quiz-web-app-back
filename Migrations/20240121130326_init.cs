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
            migrationBuilder.DropForeignKey(
                name: "FK_card_answers_Quizes_QuizId",
                table: "card_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_card_answers_quiz_cards_CardId",
                table: "card_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_card_answers_users_UserId",
                table: "card_answers");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestionRelation_QuizQuestions_QuestionId",
                table: "QuizQuestionRelation");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestionRelation_quiz_cards_CardId",
                table: "QuizQuestionRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Quizes",
                table: "Quizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuizQuestions",
                table: "QuizQuestions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuizQuestionRelation",
                table: "QuizQuestionRelation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_card_answers",
                table: "card_answers");

            migrationBuilder.DropIndex(
                name: "IX_card_answers_UserId",
                table: "card_answers");

            migrationBuilder.RenameTable(
                name: "Quizes",
                newName: "quizes");

            migrationBuilder.RenameTable(
                name: "QuizQuestions",
                newName: "quiz_questions");

            migrationBuilder.RenameTable(
                name: "QuizQuestionRelation",
                newName: "quiz_question_relation");

            migrationBuilder.RenameTable(
                name: "card_answers",
                newName: "CardAnswer");

            migrationBuilder.RenameIndex(
                name: "IX_QuizQuestionRelation_CardId",
                table: "quiz_question_relation",
                newName: "IX_quiz_question_relation_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_card_answers_QuizId",
                table: "CardAnswer",
                newName: "IX_CardAnswer_QuizId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CardAnswer",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddPrimaryKey(
                name: "PK_quizes",
                table: "quizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_quiz_questions",
                table: "quiz_questions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_quiz_question_relation",
                table: "quiz_question_relation",
                columns: new[] { "QuestionId", "CardId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_CardAnswer",
                table: "CardAnswer",
                columns: new[] { "UserId", "QuizId", "CardId" });

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

            migrationBuilder.CreateIndex(
                name: "IX_quizes_CreatorId",
                table: "quizes",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CardAnswer_CardId",
                table: "CardAnswer",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuizCard_QuizesId",
                table: "QuizQuizCard",
                column: "QuizesId");

            migrationBuilder.AddForeignKey(
                name: "FK_CardAnswer_quiz_cards_CardId",
                table: "CardAnswer",
                column: "CardId",
                principalTable: "quiz_cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_quiz_question_relation_quiz_cards_CardId",
                table: "quiz_question_relation",
                column: "CardId",
                principalTable: "quiz_cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quiz_question_relation_quiz_questions_QuestionId",
                table: "quiz_question_relation",
                column: "QuestionId",
                principalTable: "quiz_questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quizes_users_CreatorId",
                table: "quizes",
                column: "CreatorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_quiz_cards_CardId",
                table: "CardAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_quizes_QuizId",
                table: "CardAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_CardAnswer_users_UserId",
                table: "CardAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_quiz_question_relation_quiz_cards_CardId",
                table: "quiz_question_relation");

            migrationBuilder.DropForeignKey(
                name: "FK_quiz_question_relation_quiz_questions_QuestionId",
                table: "quiz_question_relation");

            migrationBuilder.DropForeignKey(
                name: "FK_quizes_users_CreatorId",
                table: "quizes");

            migrationBuilder.DropTable(
                name: "QuizQuizCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quizes",
                table: "quizes");

            migrationBuilder.DropIndex(
                name: "IX_quizes_CreatorId",
                table: "quizes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quiz_questions",
                table: "quiz_questions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quiz_question_relation",
                table: "quiz_question_relation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CardAnswer",
                table: "CardAnswer");

            migrationBuilder.DropIndex(
                name: "IX_CardAnswer_CardId",
                table: "CardAnswer");

            migrationBuilder.RenameTable(
                name: "quizes",
                newName: "Quizes");

            migrationBuilder.RenameTable(
                name: "quiz_questions",
                newName: "QuizQuestions");

            migrationBuilder.RenameTable(
                name: "quiz_question_relation",
                newName: "QuizQuestionRelation");

            migrationBuilder.RenameTable(
                name: "CardAnswer",
                newName: "card_answers");

            migrationBuilder.RenameIndex(
                name: "IX_quiz_question_relation_CardId",
                table: "QuizQuestionRelation",
                newName: "IX_QuizQuestionRelation_CardId");

            migrationBuilder.RenameIndex(
                name: "IX_CardAnswer_QuizId",
                table: "card_answers",
                newName: "IX_card_answers_QuizId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "card_answers",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Quizes",
                table: "Quizes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuizQuestions",
                table: "QuizQuestions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuizQuestionRelation",
                table: "QuizQuestionRelation",
                columns: new[] { "QuestionId", "CardId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_card_answers",
                table: "card_answers",
                columns: new[] { "CardId", "UserId", "QuizId" });

            migrationBuilder.CreateIndex(
                name: "IX_card_answers_UserId",
                table: "card_answers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_card_answers_Quizes_QuizId",
                table: "card_answers",
                column: "QuizId",
                principalTable: "Quizes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_answers_quiz_cards_CardId",
                table: "card_answers",
                column: "CardId",
                principalTable: "quiz_cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_card_answers_users_UserId",
                table: "card_answers",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestionRelation_QuizQuestions_QuestionId",
                table: "QuizQuestionRelation",
                column: "QuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestionRelation_quiz_cards_CardId",
                table: "QuizQuestionRelation",
                column: "CardId",
                principalTable: "quiz_cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
