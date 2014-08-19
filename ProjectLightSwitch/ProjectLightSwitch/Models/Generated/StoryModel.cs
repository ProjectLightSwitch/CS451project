namespace ProjectLightSwitch.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class StoryModel : DbContext
    {
        public StoryModel()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<LocalizedStoryType> LocalizedStoryTypes { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<StoryResponseRating> StoryResponseRatings { get; set; }
        public virtual DbSet<StoryResponse> StoryResponses { get; set; }
        public virtual DbSet<StoryType> StoryTypes { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<TagTree> TagTree { get; set; }
        public virtual DbSet<TranslatedString> TranslatedStrings { get; set; }
        public virtual DbSet<TranslatedTag> TranslatedTags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Language>()
                .Property(e => e.Code)
                .IsUnicode(false);

            modelBuilder.Entity<Language>()
                .HasMany(e => e.TranslatedStrings)
                .WithRequired(e => e.Language)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LocalizedStoryType>()
                .HasMany(e => e.StoryResponses)
                .WithRequired(e => e.LocalizedStoryType)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Question>()
                .HasMany(e => e.Answers)
                .WithOptional(e => e.Question)
                .WillCascadeOnDelete();

            modelBuilder.Entity<StoryResponse>()
                .Property(e => e.Gender)
                .IsFixedLength();

            modelBuilder.Entity<StoryResponse>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.StoryResponses)
                .Map(m => m.ToTable("StoryResponseTags", "pls").MapLeftKey("StoryResponseId").MapRightKey("TagId"));

            modelBuilder.Entity<StoryType>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.StoryTypes)
                .Map(m => m.ToTable("StoryTypeTags", "pls").MapLeftKey("StoryTypeId").MapRightKey("TagId"));

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
        }
    }
}
