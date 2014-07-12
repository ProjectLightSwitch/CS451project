namespace TestSite.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class TagModel : DbContext
    {
        public TagModel()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<TagTree> TagTree { get; set; }
        public virtual DbSet<TagsTranslated> TranslatedTags { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Survey>()
                .Property(e => e.Sex)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Survey>()
                .HasMany(e => e.Tags)
                .WithMany(e => e.Surveys)
                .Map(m => m.ToTable("SurveyTags", "testing").MapLeftKey("SurveyId").MapRightKey("TagId"));

            modelBuilder.Entity<Tag>()
                .Property(e => e.EnglishText)
                .IsUnicode(false);

            modelBuilder.Entity<Tag>()
                .HasMany(e => e.AncestorTags)
                .WithRequired(e => e.Ancestor)
                .HasForeignKey(e => e.AncestorId);

            modelBuilder.Entity<Tag>()
                .HasMany(e => e.DescendantTags)
                .WithRequired(e => e.Descendant)
                .HasForeignKey(e => e.DescendantId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TagsTranslated>()
                .Property(e => e.LanguageCode)
                .IsUnicode(false);
        }
    }
}
