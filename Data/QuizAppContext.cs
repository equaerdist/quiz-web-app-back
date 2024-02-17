using Core.Models;
using Microsoft.EntityFrameworkCore;
using quiz_web_app.Infrastructure;
using quiz_web_app.Models;
using Serilog;
using System.ComponentModel.DataAnnotations.Schema;

namespace quiz_web_app.Data
{
    public class QuizAppContext : DbContext
    {
        private readonly AppConfig _cfg;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Completed> CompletedQuizes { get;set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Quiz> Quizes { get; set; } = null!;
        public DbSet<QuizCard> QuizCards { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;

        public QuizAppContext(AppConfig cfg) 
        {
            _cfg = cfg; 
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_cfg.ConnectionString, options =>
            {
                options.EnableRetryOnFailure(1);
            });
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(x => x.AddConsole()));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Users
            modelBuilder.Entity<User>()
                        .HasKey(u => u.Id);

            modelBuilder.Entity<User>()
                        .ToTable("users");

            modelBuilder.Entity<User>()
                .HasMany(u => u.Created)
                .WithOne(q => q.Creator)
                .HasForeignKey(q => q.CreatorId);
            #endregion
            #region quiz_cards
            modelBuilder.Entity<QuizCard>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<QuizCard>()
                .ToTable("quiz_cards");
            modelBuilder.Entity<QuizCard>()
                .HasMany(qc => qc.Questions)
                .WithMany(qs => qs.QuizCards)
                .UsingEntity<QuizQuestionRelation>(r =>
                {
                    r.HasOne(r => r.Card)
                       .WithMany(qc => qc.QuestionsRelationships)
                       .HasForeignKey(r => r.CardId);

                    r.HasOne(r => r.Question)
                        .WithMany(qc => qc.QuizCardRelations)
                        .HasForeignKey(r => r.QuestionId);

                    r.HasKey(r => new { r.QuestionId, r.CardId });
                    r.Property(r => r.Type).IsRequired().HasMaxLength(10);
                    r.ToTable("quiz_question_relation");
                });
            #endregion
            #region pays
            modelBuilder.Entity<Pay>()
                    .HasKey(a => a.Id);

            modelBuilder.Entity<Pay>().ToTable("pays");

            modelBuilder.Entity<Pay>()
                .HasOne(p => p.User)
                .WithMany(u => u.Pays)
                .HasForeignKey(p => p.UserId);
            #endregion
            #region quizes
            modelBuilder.Entity<Quiz>()
               .ToTable("quizes");
            #endregion
            #region quiz_questions
            modelBuilder.Entity<QuizQuestion>()
              .ToTable("quiz_questions");
            #endregion
            modelBuilder.Entity<CardAnswer>()   
                .HasKey(ca => new { ca.CompletedId, ca.CardId });

            modelBuilder.Entity<Completed>().HasKey(c => new { c.QuizId, c.UserId, c.EndTime });
        }
    }
}
