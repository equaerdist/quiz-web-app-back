﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using quiz_web_app.Data;

#nullable disable

namespace quizwebapp.Migrations
{
    [DbContext(typeof(QuizAppContext))]
    partial class QuizAppContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Core.Models.CardAnswer", b =>
                {
                    b.Property<Guid>("CompletedId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CardId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("CompletedEndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CompletedQuizId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CompletedUserId")
                        .HasColumnType("uuid");

                    b.Property<TimeSpan>("Elapsed")
                        .HasColumnType("interval");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Type")
                        .HasColumnType("boolean");

                    b.HasKey("CompletedId", "CardId");

                    b.HasIndex("CardId");

                    b.HasIndex("CompletedQuizId", "CompletedUserId", "CompletedEndTime");

                    b.ToTable("CardAnswer");
                });

            modelBuilder.Entity("Core.Models.Completed", b =>
                {
                    b.Property<Guid>("QuizId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("Elapsed")
                        .HasColumnType("interval");

                    b.Property<bool>("Fulfilled")
                        .HasColumnType("boolean");

                    b.Property<int?>("Raiting")
                        .HasColumnType("integer");

                    b.Property<int>("Score")
                        .HasColumnType("integer");

                    b.HasKey("QuizId", "UserId", "EndTime");

                    b.HasIndex("UserId");

                    b.ToTable("CompletedQuizes");
                });

            modelBuilder.Entity("Core.Models.Image", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Images");
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

                    b.Property<bool>("Type")
                        .HasMaxLength(10)
                        .HasColumnType("boolean");

                    b.HasKey("QuestionId", "CardId");

                    b.HasIndex("CardId");

                    b.ToTable("quiz_question_relation", (string)null);
                });

            modelBuilder.Entity("ImageUser", b =>
                {
                    b.Property<Guid>("AccessUsersId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ImagesId")
                        .HasColumnType("uuid");

                    b.HasKey("AccessUsersId", "ImagesId");

                    b.HasIndex("ImagesId");

                    b.ToTable("ImageUser");
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

                    b.Property<long>("Completed")
                        .HasColumnType("bigint");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid");

                    b.Property<int>("Mode")
                        .HasColumnType("integer");

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

                    b.Property<string>("Thumbnail")
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

                    b.HasOne("Core.Models.Completed", "Completed")
                        .WithMany("Answers")
                        .HasForeignKey("CompletedQuizId", "CompletedUserId", "CompletedEndTime");

                    b.Navigation("Card");

                    b.Navigation("Completed");
                });

            modelBuilder.Entity("Core.Models.Completed", b =>
                {
                    b.HasOne("quiz_web_app.Models.Quiz", "Quiz")
                        .WithMany("CompletedQuizes")
                        .HasForeignKey("QuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("quiz_web_app.Models.User", "User")
                        .WithMany("CompletedQuizes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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

            modelBuilder.Entity("ImageUser", b =>
                {
                    b.HasOne("quiz_web_app.Models.User", null)
                        .WithMany()
                        .HasForeignKey("AccessUsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Core.Models.Image", null)
                        .WithMany()
                        .HasForeignKey("ImagesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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

            modelBuilder.Entity("Core.Models.Completed", b =>
                {
                    b.Navigation("Answers");
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
                    b.Navigation("CompletedQuizes");
                });

            modelBuilder.Entity("quiz_web_app.Models.User", b =>
                {
                    b.Navigation("CompletedQuizes");

                    b.Navigation("Created");

                    b.Navigation("Pays");
                });
#pragma warning restore 612, 618
        }
    }
}
