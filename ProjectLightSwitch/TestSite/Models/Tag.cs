namespace TestSite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("testing.Tags")]
    public partial class Tag
    {
        public Tag()
        {
            TranslatedTags = new HashSet<TagsTranslated>();
            AncestorTags = new HashSet<TagTree>();
            DescendantTags = new HashSet<TagTree>();
            Surveys = new HashSet<Survey>();
        }

        public int TagId { get; set; }

        public byte TagType { get; set; }

        [Required]
        [StringLength(1024)]
        public string EnglishText { get; set; }

        public virtual ICollection<TagsTranslated> TranslatedTags { get; set; }

        public virtual ICollection<TagTree> AncestorTags { get; set; }

        public virtual ICollection<TagTree> DescendantTags { get; set; }

        public virtual ICollection<Survey> Surveys { get; set; }
    }
}
