namespace ProjectLightSwitch.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class StoryModel : DbContext
    {
        public StoryModel()
            : base("name=StoryModel")
        {
        }

        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<StoryInformation> StoryInformation { get; set; }
        public virtual DbSet<StoryType> StoryTypes { get; set; }
        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<TagTree> TagTrees { get; set; }
        public virtual DbSet<TagsTranslated> TranslatedTags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Answer>()
                .Property(e => e.AnswerText)
                .IsUnicode(false);

            modelBuilder.Entity<Language>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<Question>()
                .Property(e => e.QuestionText)
                .IsUnicode(false);

            modelBuilder.Entity<Question>()
                .HasMany(e => e.Answers)
                .WithOptional(e => e.Question)
                .WillCascadeOnDelete();

            modelBuilder.Entity<StoryInformation>()
                .Property(e => e.Gender)
                .IsFixedLength();

            modelBuilder.Entity<StoryInformation>()
                .Property(e => e.Story)
                .IsUnicode(false);

            modelBuilder.Entity<StoryInformation>()
                .HasOptional(e => e.StoryInformation1)
                .WithRequired(e => e.StoryInformation2);

            modelBuilder.Entity<StoryType>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<StoryType>()
                .HasMany(e => e.Questions)
                .WithOptional(e => e.StoryType)
                .WillCascadeOnDelete();

            modelBuilder.Entity<Survey>()
                .Property(e => e.Sex)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Survey>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Surveys)
                .Map(m => m.ToTable("SurveyTags", "pls").MapLeftKey("SurveyId").MapRightKey("TagId"));

            modelBuilder.Entity<Tag>()
                .Property(e => e.EnglishText)
                .IsUnicode(false);

            modelBuilder.Entity<Tag>()
                .HasMany(e => e.Ancestors)
                .WithRequired(e => e.Ancestor)
                .HasForeignKey(e => e.AncestorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Tag>()
                .HasMany(e => e.Descendants)
                .WithRequired(e => e.Descendant)
                .HasForeignKey(e => e.DescendantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TagsTranslated>()
                .Property(e => e.LanguageCode)
                .IsUnicode(false);
        }
    }
}
