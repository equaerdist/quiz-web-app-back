﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using quiz_web_app.Data;

#nullable disable

namespace quizwebapp.Migrations
{
    [DbContext(typeof(QuizAppContext))]
    [Migration("20240121130326_init")]
    partial class init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Core.Models.CardAnswer", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("QuizId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CardId")
                        .HasColumnType("uuid");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId", "QuizId", "CardId");

                    b.HasIndex("CardId");

                    b.HasIndex("QuizId");

                    b.ToTable("CardAnswer");
                });

            modelBuilder.Entity("Core.Models.Pay", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Paid")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("pays", (string)null);
                });

            modelBuilder.Entity("Core.Models.QuizCard", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Thumbnail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("quiz_cards", (string)null);
                });

            modelBuilder.Entity("Core.Models.QuizQuestion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Thumbnail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("quiz_questions", (string)null);
                });

            modelBuilder.Entity("Core.Models.QuizQuestionRelation", b =>
                {
                    b.Property<Guid>("QuestionId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CardId")
                        .HasColumnType("uuid");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.HasKey("QuestionId", "CardId");

                    b.HasIndex("CardId");

                    b.ToTable("quiz_question_relation", (string)null);
                });

            modelBuilder.Entity("QuizQuizCard", b =>
                {
                    b.Property<Guid>("QuizCardsId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("QuizesId")
                        .HasColumnType("uuid");

                    b.HasKey("QuizCardsId", "QuizesId");

                    b.HasIndex("QuizesId");

                    b.ToTable("QuizQuizCard");
                });

            modelBuilder.Entity("quiz_web_app.Models.Quiz", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Award")
                        .HasColumnType("integer");

                    b.Property<string>("Category")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid");

                    b.Property<string>("Mode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("QuestionsAmount")
                        .HasColumnType("integer");

                    b.Property<string>("Thumbnail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("quizes", (string)null);
                });

            modelBuilder.Entity("quiz_web_app.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("Accepted")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Core.Models.CardAnswer", b =>
                {
                    b.HasOne("Core.Models.QuizCard", "Card")
                        .WithMany("CardAnswers")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("quiz_web_app.Models.Quiz", "Quiz")
                        .WithMany("CardAnswers")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("quiz_web_app.Models.User", "User")
                        .WithMany("Answers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Quiz");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Core.Models.Pay", b =>
                {
                    b.HasOne("quiz_web_app.Models.User", "User")
                        .WithMany("Pays")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Core.Models.QuizQuestionRelation", b =>
                {
                    b.HasOne("Core.Models.QuizCard", "Card")
                        .WithMany("QuestionsRelationships")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Core.Models.QuizQuestion", "Question")
                        .WithMany("QuizCardRelations")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Question");
                });

            modelBuilder.Entity("QuizQuizCard", b =>
                {
                    b.HasOne("Core.Models.QuizCard", null)
                        .WithMany()
                        .HasForeignKey("QuizCardsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("quiz_web_app.Models.Quiz", null)
                        .WithMany()
                        .HasForeignKey("QuizesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("quiz_web_app.Models.Quiz", b =>
                {
                    b.HasOne("quiz_web_app.Models.User", "Creator")
                        .WithMany("Created")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Creator");
                });

            modelBuilder.Entity("Core.Models.QuizCard", b =>
                {
                    b.Navigation("CardAnswers");

                    b.Navigation("QuestionsRelationships");
                });

            modelBuilder.Entity("Core.Models.QuizQuestion", b =>
                {
                    b.Navigation("QuizCardRelations");
                });

            modelBuilder.Entity("quiz_web_app.Models.Quiz", b =>
                {
                    b.Navigation("CardAnswers");
                });

            modelBuilder.Entity("quiz_web_app.Models.User", b =>
                {
                    b.Navigation("Answers");

                    b.Navigation("Created");

                    b.Navigation("Pays");
                });
#pragma warning restore 612, 618
        }
    }
}
